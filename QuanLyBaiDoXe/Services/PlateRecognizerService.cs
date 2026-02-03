using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QuanLyBaiDoXe.Services
{
    public class PlateRecognizerService : ILicensePlateRecognitionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PlateRecognizerService> _logger;
        private readonly string _apiToken;
        private readonly string _apiUrl;

        public PlateRecognizerService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PlateRecognizerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Lấy API token từ appsettings.json
            _apiToken = configuration["PlateRecognizer:ApiToken"] ?? "";
            _apiUrl = configuration["PlateRecognizer:ApiUrl"] ?? "https://api.platerecognizer.com/v1/plate-reader/";
        }

        public async Task<LicensePlateResult> RecognizePlateAsync(string base64Image)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiToken))
                {
                    _logger.LogWarning("PlateRecognizer API token chưa được cấu hình");
                    return new LicensePlateResult
                    {
                        Success = false,
                        ErrorMessage = "API token chưa được cấu hình. Vui lòng kiểm tra appsettings.json"
                    };
                }

                // Loại bỏ prefix data:image/... nếu có
                var imageData = base64Image;
                if (imageData.Contains(","))
                {
                    imageData = imageData.Split(',')[1];
                }

                // Tạo request
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(imageData), "upload");
                content.Add(new StringContent("vn"), "regions"); // Chỉ nhận dạng biển số Việt Nam

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {_apiToken}");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("PlateRecognizer API response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("PlateRecognizer API error: {StatusCode} - {Content}", 
                        response.StatusCode, responseContent);
                    return new LicensePlateResult
                    {
                        Success = false,
                        ErrorMessage = $"Lỗi API: {response.StatusCode}"
                    };
                }

                // Parse response
                var result = JsonSerializer.Deserialize<PlateRecognizerResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Results == null || result.Results.Count == 0)
                {
                    return new LicensePlateResult
                    {
                        Success = false,
                        ErrorMessage = "Không tìm thấy biển số trong ảnh"
                    };
                }

                var plateResult = result.Results[0];
                var rawPlate = plateResult.Plate?.ToUpper() ?? "";
                var formattedPlate = FormatVietnamesePlate(rawPlate);

                return new LicensePlateResult
                {
                    Success = true,
                    PlateNumber = formattedPlate,
                    RawPlateNumber = rawPlate,
                    Confidence = (plateResult.Score ?? 0) * 100,
                    VehicleType = plateResult.Vehicle?.Type,
                    Region = plateResult.Region?.Code
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nhận dạng biển số");
                return new LicensePlateResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<LicensePlateResult> RecognizePlateFromFileAsync(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    return new LicensePlateResult
                    {
                        Success = false,
                        ErrorMessage = "File ảnh không tồn tại"
                    };
                }

                var imageBytes = await File.ReadAllBytesAsync(imagePath);
                var base64 = Convert.ToBase64String(imageBytes);
                return await RecognizePlateAsync(base64);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đọc file ảnh");
                return new LicensePlateResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi đọc file: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Format biển số Việt Nam theo chuẩn (VD: 59A-12345 hoặc 59A-123.45)
        /// </summary>
        private string FormatVietnamesePlate(string rawPlate)
        {
            if (string.IsNullOrEmpty(rawPlate))
                return rawPlate;

            // Loại bỏ các ký tự không hợp lệ
            rawPlate = Regex.Replace(rawPlate, @"[^A-Z0-9]", "");

            // Format biển số 2 dòng hoặc 1 dòng
            // Biển số xe máy: 59A1-12345 hoặc 59A-12345
            // Biển số ô tô: 59A-12345

            if (rawPlate.Length >= 7)
            {
                // Tìm vị trí bắt đầu của số
                var match = Regex.Match(rawPlate, @"^(\d{2})([A-Z]\d?)(\d+)$");
                if (match.Success)
                {
                    var province = match.Groups[1].Value;
                    var series = match.Groups[2].Value;
                    var number = match.Groups[3].Value;

                    // Format: XXY-ZZZZZ hoặc XXY1-ZZZZZ
                    return $"{province}{series}-{number}";
                }
            }

            return rawPlate;
        }
    }

    #region API Response Models

    public class PlateRecognizerResponse
    {
        public List<PlateResult>? Results { get; set; }
    }

    public class PlateResult
    {
        public string? Plate { get; set; }
        public double? Score { get; set; }
        public RegionInfo? Region { get; set; }
        public VehicleInfo? Vehicle { get; set; }
        public List<PlateCandidate>? Candidates { get; set; }
    }

    public class RegionInfo
    {
        public string? Code { get; set; }
        public double? Score { get; set; }
    }

    public class VehicleInfo
    {
        public string? Type { get; set; }
        public double? Score { get; set; }
    }

    public class PlateCandidate
    {
        public string? Plate { get; set; }
        public double? Score { get; set; }
    }

    #endregion
}

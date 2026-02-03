using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QuanLyBaiDoXe.Services
{
    /// <summary>
    /// Service xử lý thanh toán qua MoMo
    /// </summary>
    public class MoMoService : IMoMoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MoMoService> _logger;

        // Cấu hình MoMo
        private readonly string _partnerCode;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _endpoint;
        private readonly string _returnUrl;
        private readonly string _notifyUrl;

        public MoMoService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<MoMoService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
                _logger = logger;

                // Đọc cấu hình từ appsettings.json
                _partnerCode = _configuration["MoMo:PartnerCode"] ?? "";
                _accessKey = _configuration["MoMo:AccessKey"] ?? "";
                _secretKey = _configuration["MoMo:SecretKey"] ?? "";
                _endpoint = _configuration["MoMo:Endpoint"] ?? "https://test-payment.momo.vn/v2/gateway/api/create";
                _returnUrl = _configuration["MoMo:ReturnUrl"] ?? "";
                // Hỗ trợ cả IpnUrl và NotifyUrl (ưu tiên IpnUrl)
                _notifyUrl = _configuration["MoMo:IpnUrl"] ?? _configuration["MoMo:NotifyUrl"] ?? "";
            }

        /// <summary>
        /// Tạo link thanh toán MoMo
        /// </summary>
        public async Task<MoMoPaymentResponse> CreatePaymentAsync(string orderId, long amount, string orderInfo)
        {
            try
            {
                var requestId = Guid.NewGuid().ToString();
                var extraData = "";
                var requestType = "captureWallet"; // QR Code và ví MoMo

                // Tạo chuỗi raw signature
                var rawSignature = $"accessKey={_accessKey}" +
                                   $"&amount={amount}" +
                                   $"&extraData={extraData}" +
                                   $"&ipnUrl={_notifyUrl}" +
                                   $"&orderId={orderId}" +
                                   $"&orderInfo={orderInfo}" +
                                   $"&partnerCode={_partnerCode}" +
                                   $"&redirectUrl={_returnUrl}" +
                                   $"&requestId={requestId}" +
                                   $"&requestType={requestType}";

                // Tạo chữ ký HMAC SHA256
                var signature = ComputeHmacSha256(rawSignature, _secretKey);

                // Tạo request body
                var requestBody = new
                {
                    partnerCode = _partnerCode,
                    partnerName = "Bãi đỗ xe thông minh",
                    storeId = _partnerCode,
                    requestId = requestId,
                    amount = amount,
                    orderId = orderId,
                    orderInfo = orderInfo,
                    redirectUrl = _returnUrl,
                    ipnUrl = _notifyUrl,
                    lang = "vi",
                    extraData = extraData,
                    requestType = requestType,
                    signature = signature
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("MoMo Request: {Request}", jsonContent);

                var response = await _httpClient.PostAsync(_endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("MoMo Response: {Response}", responseContent);

                var momoResponse = JsonSerializer.Deserialize<MoMoApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (momoResponse == null)
                {
                    return new MoMoPaymentResponse
                    {
                        Success = false,
                        Message = "Không thể đọc phản hồi từ MoMo"
                    };
                }

                return new MoMoPaymentResponse
                {
                    Success = momoResponse.ResultCode == 0,
                    PayUrl = momoResponse.PayUrl,
                    QrCodeUrl = momoResponse.QrCodeUrl,
                    DeepLink = momoResponse.Deeplink,
                    RequestId = requestId,
                    OrderId = orderId,
                    Message = momoResponse.Message,
                    ResultCode = momoResponse.ResultCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo thanh toán MoMo");
                return new MoMoPaymentResponse
                {
                    Success = false,
                    Message = $"Lỗi kết nối MoMo: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Xác minh chữ ký callback từ MoMo
        /// </summary>
        public bool VerifySignature(MoMoCallbackRequest request)
        {
            try
            {
                var rawSignature = $"accessKey={_accessKey}" +
                                   $"&amount={request.Amount}" +
                                   $"&extraData={request.ExtraData}" +
                                   $"&message={request.Message}" +
                                   $"&orderId={request.OrderId}" +
                                   $"&orderInfo={request.OrderInfo}" +
                                   $"&orderType={request.OrderType}" +
                                   $"&partnerCode={request.PartnerCode}" +
                                   $"&payType={request.PayType}" +
                                   $"&requestId={request.RequestId}" +
                                   $"&responseTime={request.ResponseTime}" +
                                   $"&resultCode={request.ResultCode}" +
                                   $"&transId={request.TransId}";

                var expectedSignature = ComputeHmacSha256(rawSignature, _secretKey);

                return expectedSignature.Equals(request.Signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác minh chữ ký MoMo");
                return false;
            }
        }

        /// <summary>
        /// Tính HMAC SHA256
        /// </summary>
        private static string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Model phản hồi từ API MoMo
        /// </summary>
        private class MoMoApiResponse
        {
            public string PartnerCode { get; set; } = string.Empty;
            public string OrderId { get; set; } = string.Empty;
            public string RequestId { get; set; } = string.Empty;
            public long Amount { get; set; }
            public long ResponseTime { get; set; }
            public string Message { get; set; } = string.Empty;
            public int ResultCode { get; set; }
            public string PayUrl { get; set; } = string.Empty;
            public string QrCodeUrl { get; set; } = string.Empty;
            public string Deeplink { get; set; } = string.Empty;
        }
    }
}

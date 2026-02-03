namespace QuanLyBaiDoXe.Services
{
    /// <summary>
    /// Interface cho dịch vụ thanh toán MoMo
    /// </summary>
    public interface IMoMoService
    {
        /// <summary>
        /// Tạo link thanh toán MoMo
        /// </summary>
        /// <param name="orderId">Mã đơn hàng (mã lượt gửi)</param>
        /// <param name="amount">Số tiền thanh toán</param>
        /// <param name="orderInfo">Thông tin đơn hàng</param>
        /// <returns>Link thanh toán MoMo hoặc null nếu thất bại</returns>
        Task<MoMoPaymentResponse> CreatePaymentAsync(string orderId, long amount, string orderInfo);

        /// <summary>
        /// Xác minh callback từ MoMo
        /// </summary>
        /// <param name="request">Dữ liệu callback từ MoMo</param>
        /// <returns>True nếu chữ ký hợp lệ</returns>
        bool VerifySignature(MoMoCallbackRequest request);
    }

    /// <summary>
    /// Response khi tạo thanh toán MoMo
    /// </summary>
    public class MoMoPaymentResponse
    {
        public bool Success { get; set; }
        public string? PayUrl { get; set; }
        public string? QrCodeUrl { get; set; }
        public string? DeepLink { get; set; }
        public string? RequestId { get; set; }
        public string? OrderId { get; set; }
        public string? Message { get; set; }
        public int ResultCode { get; set; }
    }

    /// <summary>
    /// Request callback từ MoMo
    /// </summary>
    public class MoMoCallbackRequest
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public long TransId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PayType { get; set; } = string.Empty;
        public long ResponseTime { get; set; }
        public string ExtraData { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }
}

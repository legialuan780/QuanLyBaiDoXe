using System.Net;
using System.Net.Mail;

namespace QuanLyBaiDoXe.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string userName)
        {
            var subject = "Mã OTP đặt lại mật khẩu - Hệ thống Quản lý Bãi Đỗ Xe";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border: 1px solid #e0e0e0; }}
        .otp-box {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 20px 40px; border-radius: 10px; display: inline-block; margin: 20px 0; }}
        .footer {{ background: #f1f1f1; padding: 20px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 10px 10px; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚗 Hệ thống Quản lý Bãi Đỗ Xe</h1>
        </div>
        <div class='content'>
            <h2>Xin chào {userName},</h2>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p>Đây là mã OTP của bạn:</p>
            <p style='text-align: center;'>
                <span class='otp-box'>{otpCode}</span>
            </p>
            <div class='warning'>
                <strong>⚠️ Lưu ý quan trọng:</strong>
                <ul>
                    <li>Mã OTP này sẽ hết hạn sau <strong>5 phút</strong>.</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</li>
                    <li>Không chia sẻ mã này với bất kỳ ai.</li>
                </ul>
            </div>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
            <p>&copy; 2024 Hệ thống Quản lý Bãi Đỗ Xe. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}

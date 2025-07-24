using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using System.Net;
using System.Net.Mail;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            var smtpClient = new SmtpClient(_config["Email:SmtpHost"])
            {
                Port = int.Parse(_config["Email:SmtpPort"]),
                Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Email:Username"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };


            if (!string.IsNullOrWhiteSpace(to))
            {
                mailMessage.To.Add(to);
            }
            else
            {
                // Tùy chọn: Ghi log cảnh báo hoặc ném ngoại lệ nếu to là null hoặc rỗng
                // Ví dụ:
                // throw new ArgumentException("Địa chỉ email người nhận không được để trống.", nameof(to));
                Console.WriteLine("Cảnh báo: Địa chỉ email người nhận không hợp lệ, không thể gửi email.");
                return; // Dừng việc gửi email nếu không có địa chỉ nhận hợp lệ
            }

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}

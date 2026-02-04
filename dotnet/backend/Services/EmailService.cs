using System.Threading.Tasks;
using EMart.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace EMart.Services
{
    // ===== Interface =====
    public interface IEmailService
    {
        Task SendLoginSuccessMailAsync(User user);
        Task SendPaymentSuccessMailAsync(Ordermaster order, byte[] invoicePdf);
    }

    // ===== Implementation =====
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ✅ Login Success Mail (Async)
        public async Task SendLoginSuccessMailAsync(User user)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:From"]));
            message.To.Add(MailboxAddress.Parse(user.Email));
            message.Subject = "Login Successful";

            message.Body = new TextPart("plain")
            {
                Text =
                    $"Hello {user.FullName},\n\n"
                    + "You have successfully logged in to E-Mart.\n\n"
                    + "Regards,\nE-Mart Team",
            };

            await SendAsync(message);
        }

        // ✅ Payment Success Mail + Invoice Attachment
        public async Task SendPaymentSuccessMailAsync(Ordermaster order, byte[] invoicePdf)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:From"]));
            message.To.Add(MailboxAddress.Parse(order.User.Email));
            message.Subject = "Payment Successful - Invoice Attached";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody =
                    $"Hello {order.User.FullName},<br/><br/>"
                    + "Your payment was successful.<br/>"
                    + "Please find your invoice attached.<br/><br/>"
                    + "Regards,<br/>E-Mart Team",
            };

            bodyBuilder.Attachments.Add(
                $"invoice_{order.Id}.pdf",
                invoicePdf,
                new ContentType("application", "pdf")
            );

            message.Body = bodyBuilder.ToMessageBody();

            await SendAsync(message);
        }

        // 🔁 Common SMTP logic (like JavaMailSender)
        private async Task SendAsync(MimeMessage message)
        {
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:Port"]),
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _configuration["EmailSettings:Username"],
                _configuration["EmailSettings:Password"]
            );

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}

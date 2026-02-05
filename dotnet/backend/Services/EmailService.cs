using EMart.Models;
using System.Net.Mail;
using System.Net;

namespace EMart.Services
{
    public interface IEmailService
    {
        Task SendLoginSuccessMailAsync(User user);
        Task SendPaymentSuccessMailAsync(Ordermaster order, byte[] invoicePdf);
        Task SendRegistrationSuccessMailAsync(User user);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false, Attachment? attachment = null)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                using (var client = new SmtpClient(host, port))
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = enableSsl;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(username ?? ""),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml
                    };
                    mailMessage.To.Add(to);

                    if (attachment != null)
                    {
                        mailMessage.Attachments.Add(attachment);
                    }

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Email sent successfully to {to}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
                // We don't throw here to avoid breaking the main business flow
            }
        }

        public async Task SendLoginSuccessMailAsync(User user)
        {
            _logger.LogInformation($"Sending login success email to {user.Email}");
            string body = $"Hello {user.FullName},\n\nYou have successfully logged in to E-Mart.\n\nRegards,\nE-Mart Team";
            await SendEmailAsync(user.Email, "Login Successful", body);
        }

        public async Task SendRegistrationSuccessMailAsync(User user)
        {
            _logger.LogInformation($"Sending registration success email to {user.Email}");
            string body = $"Hello {user.FullName},\n\nYou have successfully registered with E-Mart.\n\nRegards,\nE-Mart Team";
            await SendEmailAsync(user.Email, "Welcome to E-Mart! - Registration Successful", body);
        }

        public async Task SendPaymentSuccessMailAsync(Ordermaster order, byte[] invoicePdf)
        {
            if (string.IsNullOrEmpty(order.User?.Email)) return;

            _logger.LogInformation($"Sending payment success email with invoice to {order.User.Email}");
            string body = $"Hello {order.User.FullName},<br/><br/>Your payment was successful.<br/>Please find your invoice attached.<br/><br/>Regards,<br/>E-Mart Team";
            
            using (var ms = new MemoryStream(invoicePdf))
            {
                var attachment = new Attachment(ms, $"invoice_{order.Id}.pdf", "application/pdf");
                await SendEmailAsync(order.User.Email, "Payment Successful - Invoice Attached", body, true, attachment);
            }
        }
    }
}

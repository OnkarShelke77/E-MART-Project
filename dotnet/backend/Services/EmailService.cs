using EMart.Models;
using System.Net.Mail;
using System.Net;

namespace EMart.Services
{
    public interface IEmailService
    {
        Task SendLoginSuccessMailAsync(User user);
        Task SendPaymentSuccessMailAsync(Ordermaster order, byte[] invoicePdf);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendLoginSuccessMailAsync(User user)
        {
            _logger.LogInformation($"Sending login success email to {user.Email}");
            // In a real scenario, you'd use SmtpClient or a service like SendGrid
            await Task.CompletedTask;
        }

        public async Task SendPaymentSuccessMailAsync(Ordermaster order, byte[] invoicePdf)
        {
            _logger.LogInformation($"Sending payment success email with invoice to {order.User?.Email}");
            // In a real scenario, you'd attach the PDF
            await Task.CompletedTask;
        }
    }
}

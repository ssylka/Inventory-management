using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

namespace Inventory_Managment.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmail(string to, string subject, string body)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(
                "2000ctac@gmail.com"
            );

            var toEmail = new EmailAddress(to);

            var msg = MailHelper.CreateSingleEmail(
                from,
                toEmail,
                subject,
                plainTextContent: body,
                htmlContent: body
            );

            await client.SendEmailAsync(msg);
        }
    }
}

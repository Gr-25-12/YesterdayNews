using Microsoft.AspNetCore.Identity.UI.Services;
using SendWithBrevo;

namespace YesterdayNews.Utils
{
    public class EmailSender : IEmailSender
    {
        private readonly BrevoClient _client;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailSender(IConfiguration configuration)
        {
            _client = new BrevoClient(configuration["Brevo:ApiKey"]);
            _fromEmail = configuration["Brevo:FromEmail"];
            _fromName = configuration["Brevo:FromName"];
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await _client.SendAsync(
                new Sender(_fromName, _fromEmail), 
                new List<Recipient> { new Recipient(email, email) },  
                subject,
                htmlMessage,
                true,
                replyTo: null

            );
        }




    }
}

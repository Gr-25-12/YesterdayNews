using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;

namespace YesterdayNews.Utils
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }




    }
}

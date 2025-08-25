using System;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Utils;

namespace SubscriptionReminderFunction
{
    public class Reminder
    {
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;

        public Reminder(ILoggerFactory loggerFactory , IEmailSender emailSender , ApplicationDbContext db)
        {
            _logger = loggerFactory.CreateLogger<Reminder>();
            _emailSender = emailSender;
            _db = db;
        }

        [Function("Reminder")]
        public async Task Run([TimerTrigger("0 0 8 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Subscription reminder function executed at: {DateTime.Now}");

            try
            {
                

                var expiringSubscriptions = await GetExpiringSubscriptionsAsync();
                _logger.LogInformation($"Found {expiringSubscriptions.Count} expiring subscriptions");

                foreach (var subscription in expiringSubscriptions)
                {
                    await SendSubscriptionEmailAsync(_emailSender, subscription);
                    await MarkReminderAsSentAsync(subscription.Id);
                    _logger.LogInformation($"Sent reminder email for subscription: {subscription.Id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in subscription reminder function: {ex.Message}");
                throw;
            }
        }

        private async Task<List<Subscription>> GetExpiringSubscriptionsAsync()
        {
           


            var startDate = DateTime.Today.AddMinutes(1); // Subscriptions ending tomorrow
            var endDate = DateTime.Today.AddDays(3);   // Subscriptions ending in next 3 days

            var subscriptions = await _db.Subscriptions
                .Include(s => s.User)
                .Where(s => s.Expires >= startDate && s.Expires <= endDate)
                .Where(s => !s.ReminderSent)
                .ToListAsync();

            return subscriptions;
        }

        private async Task SendSubscriptionEmailAsync(IEmailSender emailSender, Subscription subscription)
        {
            var userName = $"{subscription.User.FirstName} {subscription.User.LastName}";
            var email = subscription.User.Email;
            var subject = "Your Subscription is Ending Soon!";
            var htmlContent = GenerateSubscriptionEmail(userName, subscription.Expires, StaticConsts.Home_URL);

            await emailSender.SendEmailAsync(email, subject, htmlContent);
        }

        private string GenerateSubscriptionEmail(string userName, DateTime? endDate , string HomeUrl)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #3A2512; max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #3A2512; padding: 20px; text-align: center; }}
                    .header img {{ max-height: 50px; }}
                    .content {{ padding: 30px; background-color: #f9f9f9; }}
                    .button {{ background-color: #3A2512; color: white !important; padding: 12px 25px; text-decoration: none; border-radius: 4px; display: inline-block; margin: 15px 0; }}
                    .footer {{ margin-top: 30px; font-size: 12px; color: #777; text-align: center; }}
                    .highlight-box {{ background-color: #fff4e5; border-left: 4px solid #ffa726; padding: 15px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <img src='https://yesterdaystoragegr12.blob.core.windows.net/notarticles/ResizedLogo.jpg' alt='Yesterday News Logo'>
                </div>
                <div class='content'>
                    <h2>Subscription Expiration Notice</h2>
                    <p>Dear {userName},</p>
                    
                    <div class='highlight-box'>
                        <p>Your Yesterday News subscription will expire on <strong>{endDate:MMMM dd, yyyy}</strong>.</p>
                    </div>
                    
                    <p>To continue enjoying uninterrupted access to our premium content and features, please renew your subscription before it expires.</p>
                    
                    <p style='text-align: center;'>
                        <a href='{HomeUrl}/' class='button'>Renew Subscription Now</a>
                    </p>
                    
                    <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
                </div>
                <div class='footer'>
                    <p>© {DateTime.Now.Year} Yesterday News. All rights reserved.</p>
                    <p>This is an automated message. Please do not reply to this email.</p>
                </div>
            </body>
            </html>";
        }

        private async Task MarkReminderAsSentAsync(int subscriptionId)
        {

            var subscription = await _db.Subscriptions.FindAsync(subscriptionId);
            if (subscription != null)
            {
                subscription.ReminderSent = true;
                await _db.SaveChangesAsync();
            }
        }
    }
}

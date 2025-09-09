using System;
using Azure;
using Azure.Data.Tables;
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
        private readonly IConfiguration _config ;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;
        private readonly TableClient _tableClient;
        public Reminder(ILoggerFactory loggerFactory , IEmailSender emailSender , ApplicationDbContext db , IConfiguration config)
        {
            _logger = loggerFactory.CreateLogger<Reminder>();
            _emailSender = emailSender;
            _db = db;
            _config = config;   
            //creatings     table is not exists
            _tableClient = new TableClient(_config["AzureWebJobsStorage"], "SentReminders");
            _tableClient.CreateIfNotExists(); 
        }

        [Function("Reminder")]
        public async Task Run([TimerTrigger("0 0 9 * * *")] TimerInfo myTimer)
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
           
            var startDate = DateTime.Today.AddMinutes(1); 
            var endDate = DateTime.Today.AddDays(3);

            var subscriptions = await _db.Subscriptions
             .Include(s => s.User)
             .Where(s => s.Expires >= startDate && s.Expires <= endDate)
                //.Where(s => !s.ReminderSent)
                .ToListAsync();

            var unsentSubscriptions = new List<Subscription>();
            foreach (var sub in subscriptions)
            {
                if (!await IsReminderSentAsync(sub.Id))
                    unsentSubscriptions.Add(sub);
            }

            foreach (var sub in unsentSubscriptions)
            {
                await SendSubscriptionEmailAsync(_emailSender, sub);
                await MarkReminderAsSentAsync(sub.Id);
            }

            return unsentSubscriptions;
        }

        private async Task SendSubscriptionEmailAsync(IEmailSender emailSender, Subscription subscription)
        {
            var userName = $"{subscription.User.FirstName} {subscription.User.LastName}";
            var email = subscription.User.Email;
            var subject = "Your Subscription is Ending Soon!";
            var htmlContent = EmailTemplate.GenerateSubscriptionReminderEmail(userName, subscription.Expires, StaticConsts.Home_URL);

            await emailSender.SendEmailAsync(email, subject, htmlContent);
        }

       
        private async Task<bool> IsReminderSentAsync(int subscriptionId)
        {
            try
            {
                var entity = await _tableClient.GetEntityAsync<SentReminderEntity>("Reminder", subscriptionId.ToString());
                return entity != null;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }
        private async Task MarkReminderAsSentAsync(int subscriptionId)
        {

            var entity = new SentReminderEntity
            {
                PartitionKey = "Reminder",
                RowKey = subscriptionId.ToString(),
                SentAt = DateTimeOffset.UtcNow
            };

            await _tableClient.UpsertEntityAsync(entity);
        }
    }
}

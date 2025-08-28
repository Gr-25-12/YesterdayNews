using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionReminderFunction
{
    class SentReminderEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Reminder";
        public string RowKey { get; set; } 
        public DateTimeOffset SentAt { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
   
}

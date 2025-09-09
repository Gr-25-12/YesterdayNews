using System.Diagnostics;
using System.Text.Json.Serialization;


namespace FinanceServices.Models.API
{
    public class Crypto
    {
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("displaySymbol")] public string? DisplaySymbol { get; set; }
        [JsonPropertyName("symbol")] public string? Symbol { get; set; }

        private readonly object _lock = new(); // for thread-safety
        private readonly Queue<decimal> priceSnapshots24 = new();
        private const int MAX_SNAPSHOTS = 1440; //1440 (change to 1 for testing)

        public decimal CurrentPrice { get; set; }
        public decimal Price24HoursAgo { get; private set; }

        public long TimeStamp { private get; set; }

        [JsonIgnore]
        public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(TimeStamp / 1000).UtcDateTime;
        public decimal Change => Price24HoursAgo != 0 ? CurrentPrice - Price24HoursAgo : 0;

        public decimal PercentageChange => Price24HoursAgo != 0
            ? (CurrentPrice - Price24HoursAgo) / Price24HoursAgo * 100
            : 0;

        public void UpdateSnapshots()
        {
            lock (_lock)
            {
                priceSnapshots24.Enqueue(CurrentPrice);

                while (priceSnapshots24.Count > MAX_SNAPSHOTS)
                    priceSnapshots24.Dequeue();

                if (priceSnapshots24.Count == MAX_SNAPSHOTS)
                    Price24HoursAgo = priceSnapshots24.Peek();
            }
        }
        public void LoadSnapshotFromTable(decimal tablePrice)
        {
            lock (_lock)
            {
                Price24HoursAgo = tablePrice;

                priceSnapshots24.Clear();
                priceSnapshots24.Enqueue(tablePrice);
            }
        }
    }
}

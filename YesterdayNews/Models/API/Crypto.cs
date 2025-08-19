using System.Diagnostics;
using System.Text.Json.Serialization;


namespace YesterdayNews.Models.API
{
    public class Crypto
    {
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("displaySymbol")] public string? DisplaySymbol { get; set; }
        [JsonPropertyName("symbol")] public string? Symbol { get; set; }

        private readonly Queue<decimal> priceSnapshots24 = new();
        private const int MAX_SNAPSHOTS = 1440;

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
            Debug.Write($"Snapshot take for {Symbol}");
            Debug.WriteLine($", Current Price: {CurrentPrice}");
            Debug.WriteLine($"----------------------------");
            priceSnapshots24.Enqueue(CurrentPrice);

            while (priceSnapshots24.Count > MAX_SNAPSHOTS)
                priceSnapshots24.Dequeue();

            if (priceSnapshots24.Count == MAX_SNAPSHOTS)
                Price24HoursAgo = priceSnapshots24.Peek();
        }
    }

}

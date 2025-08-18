using System.Text.Json.Serialization;
using YesterdayNews.Utils;

namespace YesterdayNews.Models.API
{
    public class Crypto
    {
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("displaySymbol")] public string DisplaySymbol { get; set; }
        [JsonPropertyName("symbol")] public string Symbol { get; set; }


        public decimal CurrentPrice { get; set; }
        public decimal Price24HoursAgo { get; private set; }
        private Queue<PriceSnapshot> priceSnapshots24 = new();
        private Timer snapshotTimer;
        public long TimeStamp { private get; set; }

        [JsonIgnore]
        public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(TimeStamp / 1000).UtcDateTime;
        public decimal Change => Price24HoursAgo != 0 ? CurrentPrice - Price24HoursAgo : 0;

        public decimal PercentageChange => Price24HoursAgo != 0
            ? (CurrentPrice - Price24HoursAgo) / Price24HoursAgo * 100
            : 0;
        private void AddSnapShot()
        {
            //ADD snapshot
            var now = DateTime.UtcNow;
            priceSnapshots24.Enqueue(new PriceSnapshot { Time = now, Price = CurrentPrice });


        }
    }

}

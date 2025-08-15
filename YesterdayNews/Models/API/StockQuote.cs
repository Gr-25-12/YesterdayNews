using System.Text.Json.Serialization;

namespace YesterdayNews.Models.API
{
    public class StockQuote
    {
        [JsonPropertyName("c")] public decimal CurrentPrice { get; set; }
        [JsonPropertyName("pc")] public decimal ClosingPrice { get; set; }
        [JsonPropertyName("t")] public long TimeStamp { private get; set; }

        [JsonIgnore]
        public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(TimeStamp/1000).UtcDateTime;
        public decimal Change => ClosingPrice != 0 ? CurrentPrice - ClosingPrice : 0;

        public decimal PercentageChange => ClosingPrice != 0
            ? (CurrentPrice - ClosingPrice) / ClosingPrice * 100
            : 0;
    }
}

using System.Text.Json.Serialization;

namespace YesterdayNews.Models.API
{
    public class StockQuote
    {
        [JsonPropertyName("c")] public decimal CurrentPrice { get; set; }
        [JsonPropertyName("d")] public decimal Change { get; set; }
        [JsonPropertyName("dp")] public decimal PercentageChange { get; set; }
        [JsonPropertyName("h")] public decimal DailyHigh { get; set; }
        [JsonPropertyName("l")] public decimal DailyLow { get; set; }
        [JsonPropertyName("o")] public decimal OpeningPrice { get; set; }
        [JsonPropertyName("pc")] public decimal ClosingPrice { get; set; }
        [JsonPropertyName("t")] private long TimeStamp { get; set; }

        public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(TimeStamp).UtcDateTime;
    }
}

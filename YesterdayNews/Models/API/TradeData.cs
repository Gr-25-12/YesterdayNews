using System.Text.Json.Serialization;

namespace YesterdayNews.Models.API
{
    public class TradeData
    {
        [JsonPropertyName("p")]
        public decimal Price { get; set; }

        [JsonPropertyName("s")]
        public string Symbol { get; set; }

        [JsonPropertyName("t")]
        public long TimeStamp { get; set; }

        [JsonPropertyName("v")]
        public decimal Volume { get; set; }
    }
}

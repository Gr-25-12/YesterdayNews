
using System.Text.Json.Serialization;

namespace YesterdayNews.Models.API
{
    public class TradesResponse
    {
        [JsonPropertyName("data")]
        public List<TradeData> Data { get; set; } = new();
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}

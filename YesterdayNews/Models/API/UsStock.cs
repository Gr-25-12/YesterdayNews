using System.Text.Json.Serialization;

namespace YesterdayNews.Models.API
{
    public class UsStock
    {
        [JsonPropertyName("currency")] public string Currency { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("displaySymbol")] public string DisplaySymbol { get; set; }
        [JsonPropertyName("figi")] public string Figi { get; set; }
        [JsonPropertyName("isin")] public object Isin { get; set; }
        [JsonPropertyName("mic")] public string Mic { get; set; }
        [JsonPropertyName("shareClassFIGI")] public string ShareClassFIGI { get; set; }
        [JsonPropertyName("symbol")] public string Symbol { get; set; }
        [JsonPropertyName("symbol2")] public string Symbol2 { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; }
    }
}

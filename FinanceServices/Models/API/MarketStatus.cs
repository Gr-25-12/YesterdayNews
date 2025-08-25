using System.Text.Json.Serialization;

namespace FinanceServices.Models.API
{
    public class MarketStatus
    {
        [JsonPropertyName("exchange")] public string Exchange { get; set; }
        [JsonPropertyName("holiday")] public string Holiday { get; set; }
        [JsonPropertyName("isOpen")] public bool IsOpen { get; set; }
        [JsonPropertyName("session")] public string Session { get; set; }
        [JsonPropertyName("t")] public long TimeStamp { private get; set; }
        [JsonPropertyName("timezone")] public string Timezone { get; set; }


        [JsonIgnore]
        public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(TimeStamp / 1000).UtcDateTime;

    }
}

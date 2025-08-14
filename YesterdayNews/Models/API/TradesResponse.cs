
namespace YesterdayNews.Models.API
{
    public class TradesResponse
    {
        public List<TradeData> Data { get; set; } = new();
        public string Type { get; set; }
    }
}

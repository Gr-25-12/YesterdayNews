using YesterdayNews.Models.API;

namespace YesterdayNews.Models.ViewModels
{
    public class MarketsVM
    {
        public Dictionary<string, StockQuote> StockPrices { get; set; } = new Dictionary<string, StockQuote>();
        public Dictionary<string, UsStock> StockInfo { get; set; } = new Dictionary<string, UsStock>();
    }
}

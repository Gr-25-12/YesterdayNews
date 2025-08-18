using YesterdayNews.Models.API;

namespace YesterdayNews.Models.ViewModels
{
    public class MarketsVM
    {
        public Dictionary<string, StockQuote> NasdaqStockPrices { get; set; } = new Dictionary<string, StockQuote>();
        public Dictionary<string, UsStock> NasdaqStockInfo { get; set; } = new Dictionary<string, UsStock>();
        public Dictionary<string, StockQuote> NyseStockPrices { get; set; } = new Dictionary<string, StockQuote>();
        public Dictionary<string, UsStock> NyseStockInfo { get; set; } = new Dictionary<string, UsStock>();
        public Dictionary<string, Crypto> CryptoPrices { get; set; } = new Dictionary<string, Crypto>();
    }
}

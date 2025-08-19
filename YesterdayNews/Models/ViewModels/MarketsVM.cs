using System.Collections.Concurrent;
using YesterdayNews.Models.API;

namespace YesterdayNews.Models.ViewModels
{
    public class MarketsVM
    {
        public Dictionary<string, StockQuote> NasdaqStockPrices { get; set; } = new Dictionary<string, StockQuote>();
        public Dictionary<string, UsStock> NasdaqStockInfo { get; set; } = new Dictionary<string, UsStock>();
        public Dictionary<string, StockQuote> NyseStockPrices { get; set; } = new Dictionary<string, StockQuote>();
        public Dictionary<string, UsStock> NyseStockInfo { get; set; } = new Dictionary<string, UsStock>();
        public ConcurrentDictionary<string, Crypto> CryptoPrices { get; set; } = new ConcurrentDictionary<string, Crypto>();
    }
}

using FinanceServices.Models.API;

namespace FinanceServices.Models
{
    public class MarketDto
    {
        public Dictionary<string, StockQuote> NasdaqStockPrices { get; set; } = new();
        public Dictionary<string, UsStock> NasdaqStockInfo { get; set; } = new();
        public Dictionary<string, StockQuote> NyseStockPrices { get; set; } = new();
        public Dictionary<string, UsStock> NyseStockInfo { get; set; } = new();
        public Dictionary<string, Crypto> CryptoPrices { get; set; } = new();
    }
}

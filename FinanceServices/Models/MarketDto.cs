using FinanceServices.Models.API;

namespace FinanceServices.Models
{
    public class MarketDto
    {
        public string UsMarketStatus { get; set; } = "Status Unknown";
        public Dictionary<string, CachedStock> NasdaqStocks { get; set; } = new();
        public Dictionary<string, CachedStock> NyseStocks { get; set; } = new();
        public Dictionary<string, Crypto> CryptoPrices { get; set; } = new();
    }
}

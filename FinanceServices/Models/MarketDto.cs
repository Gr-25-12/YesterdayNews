using FinanceServices.Models.API;

namespace FinanceServices.Models
{
    public class MarketDto
    {
        public string UsMarketStatus { get; set; } = "Status Unknown";
        public List<CachedStock> NasdaqStocks { get; set; } = new();
        public List<CachedStock> NyseStocks { get; set; } = new();
        public List<Crypto> CryptoPrices { get; set; } = new();
        public List<Forex> Currencies { get; set; } = new();
        public List<Forex> Commodities { get; set; } = new();
    }
}

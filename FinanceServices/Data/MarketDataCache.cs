using FinanceServices.Models.API;
using System.Collections.Concurrent;

namespace FinanceServices.Data
{
    public class MarketDataCache
    {
        public ConcurrentDictionary<string, MarketStatus> MarketStatus { get; private set; } = new();
        public ConcurrentDictionary<string, StockQuote> StockQuotes { get; private set; } = new();
        public ConcurrentDictionary<string, UsStock> UsStocks { get; private set; } = new();
        public ConcurrentDictionary<string, Crypto> CryptoQuotes { get; private set; } = new();

        public StockQuote? GetCachedStockQuote(string symbol)
        {
            return StockQuotes.TryGetValue(symbol, out var quote) ? quote : null;
        }
        public UsStock? GetCachedUsStock(string symbol)
        {
            return UsStocks.TryGetValue(symbol, out var info) ? info : null;
        }
        public Crypto? GetCachedCryptoQuote(string symbol)
        {
            return CryptoQuotes.TryGetValue(symbol, out var quote) ? quote : null;
        }
    }
}

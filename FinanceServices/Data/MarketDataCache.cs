using FinanceServices.Models;
using FinanceServices.Models.API;
using FinanceServices.Utilities;
using System.Collections.Concurrent;

namespace FinanceServices.Data
{
    public class MarketDataCache
    {
        public ConcurrentDictionary<string, MarketStatus> MarketStatus { get; private set; } = new();
        public ConcurrentDictionary<string, CachedStock> Stocks { get; private set; } = new();
        public ConcurrentDictionary<string, Crypto> CryptoQuotes { get; private set; } = new();
        public ConcurrentDictionary<string, Forex> Currencies { get; private set; } = new();
        public ConcurrentDictionary<string, Forex> Commodities { get; private set; } = new();
        public MarketStatus GetCachedMarketStatus(string exchange)
        {
            return MarketStatus.TryGetValue(exchange, out var status) ? status : null;
        }
        public Dictionary<string, CachedStock> GetNasdaqStocks()
        {
            return Stocks.Where(kvp => kvp.Value.Exchange == FinanceConstants.NASDAQ)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public Dictionary<string, CachedStock> GetNyseStocks()
        {
            return Stocks.Where(kvp => kvp.Value.Exchange == FinanceConstants.NYSE)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public Dictionary<string, Crypto> GetCryptos()
        {
            return CryptoQuotes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public CachedStock GetCachedStock(string symbol)
        {
            return Stocks.TryGetValue(symbol, out var stock) ? stock : null;
        }
        public Crypto GetCachedCryptoQuote(string symbol)
        {
            return CryptoQuotes.TryGetValue(symbol, out var quote) ? quote : null;
        }
        public Forex GetCachedCurrencyQuote(string symbol)
        {
            return Currencies.TryGetValue(symbol, out var quote) ? quote : null;
        }
        public Forex GetCachedCommodityQuote(string symbol)
        {
            return Commodities.TryGetValue(symbol, out var quote) ? quote : null;
        }
        public string[] GetAllSymbols()
        {
            return Stocks.Keys
                .Concat(CryptoQuotes.Keys
                .Concat(Currencies.Keys
                .Concat(Commodities.Keys)))
                .ToArray();
        }
    }
}

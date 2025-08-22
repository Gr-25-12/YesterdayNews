using FinanceServices.Data;
using FinanceServices.Models;
using FinanceServices.Models.API;
using FinanceServices.Services.BackgroundServices;
using FinanceServices.Services.IServices;
using FinanceServices.Utilities;
using Microsoft.Extensions.Logging;


namespace FinanceServices.Services
{
    public class FinanceApiServices : IFinanceApiServices
    {
        private readonly MarketDataCache _marketDataCache;
        private readonly ILogger<FinanceApiServices> _logger;
        public FinanceApiServices(MarketDataCache cache, ILogger<FinanceApiServices> logger)
        {
            _marketDataCache = cache;
            _logger = logger;
        }

        public MarketDto GetMarketsModel(string[] symbols = null)
        {
            string marketStatus = "Status Unknown";
            if (_marketDataCache.MarketStatus.TryGetValue(FinanceConstants.US, out var value))
            {
                marketStatus = GetMarketStatusAsString(value);
            }
            string[] stockSymbols = _marketDataCache.StockQuotes.Keys.ToArray();
            string[] cryptoSymbols = _marketDataCache.CryptoQuotes.Keys.ToArray();
            string[] allSymbols = stockSymbols.Concat(cryptoSymbols).ToArray();

            MarketDto model = new MarketDto();
            model.UsMarketStatus = marketStatus;
            var symbolsToUse = (symbols == null || symbols.Length == 0) ? allSymbols : symbols;

            if (symbolsToUse != null)
            {
                foreach (var symbol in symbolsToUse)
                {
                    SetMarketModel(ref model, symbol);
                }
            }
            return model;
        }
        public string[] GetSmallSymbolList()
        {
            return FinanceConstants.SmallSymbolsList;
        }
        public string GetMarketStatus(string exchange)
        {
            if(exchange.ToUpper() == "US" || exchange.ToUpper() == FinanceConstants.US)
            {
                if (_marketDataCache.MarketStatus.TryGetValue(FinanceConstants.US, out var value))
                {
                    return GetMarketStatusAsString(value);
                }
                
            }
            _logger.LogError($"{exchange} Does not exist. please use US");
            return "Status Unknown";
        }

        private string GetMarketStatusAsString(MarketStatus value)
        {
            string reply = string.Empty;
            if (value.IsOpen)
                reply = "Open";
            else if (value.Session == "pre-market" || value.Session == "post-market")
                reply = value.Session;
            else
                reply = "Closed";
            return reply;
        }

        private void SetMarketModel(ref MarketDto model, string symbol)
        {
            var stockinfo = _marketDataCache.GetCachedUsStock(symbol);

            if (stockinfo != null)
            {
                var quote = _marketDataCache.GetCachedStockQuote(symbol);
                if (quote != null)
                {
                    if (stockinfo.Mic == FinanceConstants.NASDAQ)
                    {
                        model.NasdaqStockPrices[symbol] = quote;
                        model.NasdaqStockInfo[symbol] = stockinfo;
                    }
                    else if (stockinfo.Mic == FinanceConstants.NYSE)
                    {
                        model.NyseStockPrices[symbol] = quote;
                        model.NyseStockInfo[symbol] = stockinfo;
                    }
                }
            }
            var cryptoInfo = _marketDataCache.GetCachedCryptoQuote(symbol);
            if (cryptoInfo != null)
            {
                model.CryptoPrices[symbol] = cryptoInfo;
            }
        }
    }
}

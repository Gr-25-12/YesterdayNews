using FinanceServices.Data;
using FinanceServices.Models;
using FinanceServices.Services.BackgroundServices;
using FinanceServices.Services.IServices;
using FinanceServices.Utilities;


namespace FinanceServices.Services
{
    public class FinanceApiServices : IFinanceApiServices
    {
        private readonly MarketDataCache _marketDataCache;
        public FinanceApiServices(MarketDataCache cache)
        {
            _marketDataCache = cache;
        }

        public MarketDto GetMarketsModel(string[] symbols = null)
        {
            string[] stockSymbols = _marketDataCache.StockQuotes.Keys.ToArray();
            string[] cryptoSymbols = _marketDataCache.CryptoQuotes.Keys.ToArray();
            string[] allSymbols = stockSymbols.Concat(cryptoSymbols).ToArray();

            MarketDto model = new MarketDto();
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

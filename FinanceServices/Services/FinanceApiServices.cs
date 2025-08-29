using FinanceServices.Data;
using FinanceServices.Models;
using FinanceServices.Models.API;
using FinanceServices.Services.BackgroundServices;
using FinanceServices.Services.IServices;
using FinanceServices.Utilities;
using Microsoft.Extensions.Logging;
using System.Reflection;


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
            var value = _marketDataCache.GetCachedMarketStatus(FinanceConstants.US);
            if (value != null)
            {
                marketStatus = GetMarketStatusAsString(value);
            }

            var allSymbols = _marketDataCache.GetAllSymbols();

            MarketDto model = new MarketDto();
            if (symbols == null)
            {
                CreateModelLists(ref model); //create full lists for full view
            }
            model.UsMarketStatus = marketStatus;

            //use allsymbols if symbols are null
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
            if (exchange.ToUpper() == "US" || exchange.ToUpper() == FinanceConstants.US)
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
            var stockInfo = _marketDataCache.GetCachedStock(symbol);
            var cryptoInfo = _marketDataCache.GetCachedCryptoQuote(symbol);
            var forexInfo = _marketDataCache.GetCachedCurrencyQuote(symbol);
            var commInfo = _marketDataCache.GetCachedCommodityQuote(symbol);
            if (stockInfo != null)
            {
                if (stockInfo.Exchange == FinanceConstants.NASDAQ)
                {
                    UpdateNasdaq(ref model, stockInfo);
                }
                else if (stockInfo.Exchange == FinanceConstants.NYSE)
                {
                    UpdateNyse(ref model, stockInfo);
                }
            }
            else if (cryptoInfo != null)
            {
                UpdateCrypto(ref model, cryptoInfo);
            }
            else if (forexInfo != null)
            {
                UpdateCurrency(ref model, forexInfo);
            }
            else if (commInfo != null)
            {
                UpdateCommodity(ref model, commInfo);
            }
        }
        private void CreateModelLists(ref MarketDto model)
        {
            foreach (var key in FinanceConstants.SortedNasdaqReference)
            {
                var stock = new CachedStock { Symbol = key };
                model.NasdaqStocks.Add(stock);
            }
            foreach (var key in FinanceConstants.SortedNyseReference)
            {
                var stock = new CachedStock { Symbol = key };
                model.NyseStocks.Add(stock);
            }
            foreach (var key in FinanceConstants.SortedCryptoReference)
            {
                var crypto = new Crypto { Symbol = key };
                model.CryptoPrices.Add(crypto);
            }
            foreach (var key in FinanceConstants.SortedCommoditiesReference)
            {
                var forex = new Forex { Symbol = key };
                model.Commodities.Add(forex);
            }
            foreach (var key in FinanceConstants.SortedCurrenciesReference)
            {
                var forex = new Forex { Symbol = key };
                model.Currencies.Add(forex);
            }
        }
        private void UpdateNasdaq(ref MarketDto model, CachedStock stockInfo)
        {
            var index = model.NasdaqStocks.FindIndex(c => c.Symbol == stockInfo.Symbol);
            if (index >= 0)
            {
                model.NasdaqStocks[index] = stockInfo;
            }
            else
                model.NasdaqStocks.Add(stockInfo);
        }
        private void UpdateNyse(ref MarketDto model, CachedStock stockInfo)
        {
            var index = model.NyseStocks.FindIndex(c => c.Symbol == stockInfo.Symbol);
            if (index >= 0)
            {
                model.NyseStocks[index] = stockInfo;
            }
            else
                model.NyseStocks.Add(stockInfo);
        }
        private void UpdateCrypto(ref MarketDto model, Crypto cryptoInfo)
        {
            var index = model.CryptoPrices.FindIndex(c => c.Symbol == cryptoInfo.Symbol);
            if (index >= 0)
            {
                model.CryptoPrices[index] = cryptoInfo;
            }
            else
                model.CryptoPrices.Add(cryptoInfo);
        }
        private void UpdateCurrency(ref MarketDto model, Forex forexInfo)
        {
            var index = model.Currencies.FindIndex(c => c.Symbol == forexInfo.Symbol);
            if (index >= 0)
            {
                model.Currencies[index] = forexInfo;
            }
            else
                model.Currencies.Add(forexInfo);
        }
        private void UpdateCommodity(ref MarketDto model, Forex commInfo)
        {
            var index = model.Commodities.FindIndex(c => c.Symbol == commInfo.Symbol);
            if (index >= 0)
            {
                model.Commodities[index] = commInfo;
            }
            else
                model.Commodities.Add(commInfo);
        }
    }
}

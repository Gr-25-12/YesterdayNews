using FinanceServices.Data;
using FinanceServices.Models;
using FinanceServices.Models.API;
using FinanceServices.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;


namespace FinanceServices.Services.BackgroundServices
{
    public class FinnhubApiService : BackgroundService
    {
        private readonly ILogger<FinnhubApiService> _logger;
        private readonly FinnhubApiCallsCounter _apiCallsCounter;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly MarketDataCache _cache;
        private static List<UsStock>? UsStocksRaw { get; set; } //full list
        private static ConcurrentDictionary<string, UsStock>? UsStocksFiltered { get; set; } = new(); //List of stocks based on SymbolList
        private static List<Crypto>? BinanceList { get; set; }
        public event Func<string, Task> OnApiMarketStatusError;
        private Dictionary<string, bool> StockErrors = new();

        public FinnhubApiService(FinnhubApiCallsCounter finnhubApiCallsCounter, HttpClient httpClient, IConfiguration config, MarketDataCache cache, ILogger<FinnhubApiService> logger)
        {
            _apiCallsCounter = finnhubApiCallsCounter;
            _httpClient = httpClient;
            _apiKey = "" + config["Finnhub:ApiKey"];
            _cache = cache;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var statusTask = RunMarketStatusLoop(stoppingToken);
            var listTask = RunUpdateListsLoop(stoppingToken);

            await Task.WhenAll(statusTask, listTask);
        }
        private async Task RunMarketStatusLoop(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateMarketStatus();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"{ex}, Failed Updating MarketStatus");
                }
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateMarketStatus(stoppingToken);
            }
        }

        private async Task RunUpdateListsLoop(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateLists();

                    //Forex CURRENCIES (just caching preset strings since there is only tradedata, no base data)
                    if (_cache.Currencies.Count() == 0)
                    {
                        CacheCurrencies();
                    }
                    //Forex COMMODITIES (just caching preset strings since there is only tradedata, no base data)
                    if (_cache.Commodities.Count() == 0)
                    {
                        CacheCommodities();
                    }
                    if (UsStocksRaw.Count > 0 && BinanceList.Count > 0)
                    {
                        //CRYPTOS
                        if (_cache.CryptoQuotes.Count() == 0)
                            CacheCryptos();

                        //STOCKS
                        foreach (var symbol in FinanceConstants.LargeSymbolsList)
                        {
                            if (!symbol.Contains("BINANCE") && !symbol.Contains(FinanceConstants.OANDA))
                            {
                                var usStockInfo = GetUsStockRaw(symbol);
                                if (usStockInfo == null)
                                    throw new Exception($"No stock info for symbol: {symbol}");
                                UsStocksFiltered[symbol] = usStockInfo;
                            }
                        }

                        await CacheStocks();
                        _logger.LogWarning("ALL CRYPTOS AND STOCKS CACHED!"); //warning cause easier to spot
                        // success, break out of loop
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"{ex}, Failed Updating Lists");
                }
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        private void CacheCurrencies()
        {
            var usdPair = FinanceConstants.usdForex;
            foreach (var pair in usdPair)
            {
                string displaySymbol = pair.Key.Split(':')[1];
                _cache.Currencies[pair.Key] = new Forex()
                {
                    Symbol = pair.Key,
                    DisplaySymbol = displaySymbol,
                    Description = pair.Value,
                };
            }
        }
        private void CacheCommodities()
        {
            var usdPair = FinanceConstants.usdComm;
            foreach (var pair in usdPair)
            {
                string displaySymbol = pair.Key.Split(':')[1];
                _cache.Commodities[pair.Key] = new Forex()
                {
                    Symbol = pair.Key,
                    DisplaySymbol = displaySymbol,
                    Description = pair.Value,
                };
            }
        }
        private void CacheCryptos()
        {
            var symbolList = FinanceConstants.LargeSymbolsList;
            foreach (var symbol in symbolList)
            {
                if (symbol.Contains("BINANCE"))
                {
                    foreach (var item in BinanceList)
                    {
                        if (item.Symbol == symbol)
                        {
                            _cache.CryptoQuotes[symbol] = item;
                            _logger.LogInformation($"Added {item.DisplaySymbol} to cached crypto");
                        }
                    }
                }
            }
        }
        private async Task CacheStocks()
        {
            foreach (var usStock in UsStocksFiltered.Values)
            {
                if (_cache.Stocks.ContainsKey(usStock.Symbol))
                    continue; //skip if it's already cached

                var quote = await GetStockQuote(usStock.Symbol); //symbolList size nr API calls
                if (quote == null)
                    throw new Exception($"No stock quote for symbol: {usStock.Symbol}");
                else
                {
                    CachedStock newStock = new CachedStock
                    {
                        Symbol = usStock.Symbol,
                        DisplayName = usStock.Description,
                        Exchange = usStock.Mic,
                        CurrentPrice = quote.CurrentPrice,
                        ClosingPrice = quote.ClosingPrice
                    };
                    _cache.Stocks[newStock.Symbol] = newStock;
                    _logger.LogInformation($"Added {newStock.Symbol} to cached stocks");
                }
                // Wait 1 minute between symbols
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        private async Task UpdateMarketStatus()
        {
            try
            {

                var marketStatus = await GetMarketStatus(FinanceConstants.US);
                if (marketStatus == null)
                    throw new Exception("null");
                _cache.MarketStatus[FinanceConstants.US] = marketStatus;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching market status: {ex.Message}");
            }
        }
        private async Task UpdateLists()
        {
            if (UsStocksRaw == null || UsStocksRaw.Count == 0)
            {
                _logger.LogInformation("Updating UsStocksRaw ");
                UsStocksRaw = await GetUsStockList();
                if (UsStocksRaw == null || UsStocksRaw.Count == 0)
                    throw new Exception($"UsStocksRaw not loaded");
            }
            if (BinanceList == null || BinanceList.Count == 0)
            {
                _logger.LogInformation("Updating BinanceList ");
                BinanceList = await GetBinanceCryptoList();
                if (BinanceList == null || BinanceList.Count == 0)
                    throw new Exception($"BinanceList not loaded");
            }
        }
        private async Task<List<UsStock>> GetUsStockList()
        {
            if (_apiCallsCounter.IsCallPossible())
            {
                string baseUrl = FinanceConstants.BaseUrl;
                string us = "stock/symbol?exchange=US";
                var url = $"{baseUrl}{us}&token={_apiKey}";
                var list = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
                return list;
            }
            else
            {
                return null;
            }
        }
        public static IReadOnlyDictionary<string, UsStock> GetFilteredStocks()
        {
            // returns null or a copy so original can't be altered
            if (UsStocksFiltered == null)
                return new Dictionary<string, UsStock>();
            return new Dictionary<string, UsStock>(UsStocksFiltered);
        }
        private async Task<List<Crypto>> GetBinanceCryptoList()
        {
            if (_apiCallsCounter.IsCallPossible())
            {
                string baseUrl = FinanceConstants.BaseUrl;
                string binance = "crypto/symbol?exchange=binance";
                string url = $"{baseUrl}{binance}&token={_apiKey}";
                var list = await _httpClient.GetFromJsonAsync<List<Crypto>>(url) ?? new List<Crypto>();
                return list;
            }
            else
            {
                return null;
            }
        }
        private async Task<StockQuote> GetStockQuote(string symbol)
        {
            if (_apiCallsCounter.IsCallPossible())
            {
                string baseUrl = FinanceConstants.BaseUrl;
                string stockQuote = $"quote?symbol={symbol}";
                var url = $"{baseUrl}{stockQuote}&token={_apiKey}";
                var quote = await _httpClient.GetFromJsonAsync<StockQuote>(url);
                return quote;
            }
            else
            {
                StockErrors[symbol] = true;
                return null;
            }
        }
        private UsStock GetUsStockRaw(string symbol)
        {
            if (UsStocksRaw == null)
                return null;

            foreach (var stock in UsStocksRaw)
            {
                if (stock.Symbol == symbol)
                    return stock;
            }
            return null;
        }

        public async Task<MarketStatus> GetMarketStatus(string exchange)
        {
            if (_apiCallsCounter.IsCallPossible())
            {
                string baseUrl = FinanceConstants.BaseUrl;

                string marketStatus = $"stock/market-status?exchange={exchange}";
                var url = $"{baseUrl}{marketStatus}&token={_apiKey}";
                var status = await _httpClient.GetFromJsonAsync<MarketStatus>(url);
                return status;
            }
            else
            {
                await OnApiMarketStatusError?.Invoke("Api Calls Limit reached!");
                return new MarketStatus();
            }
        }

        //public async Task<string> GetForexQuotes(string exchangeName)
        //{
        //    throw new NotImplementedException();
        //}
        //public async Task<CompanyProfile> GetCompanyProfile(string tickerSymbol)
        //{
        //    string profileLink = $"/stock/profile2?symbol={tickerSymbol}";
        //    var url = $"{_baseUrl}{profileLink}{_apiKey}";
        //    var profile = await _httpClient.GetFromJsonAsync<CompanyProfile>(url);
        //    return profile;
        //}

        //public async Task<string> GetCompanyFinancials(string tickerSymbol)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<string> GetCompanyNews(string tickerSymbol)
        //{
        //    throw new NotImplementedException();
        //}
        //public async Task<string> GetMarketNews(string marketType)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

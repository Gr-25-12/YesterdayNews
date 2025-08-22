using FinanceServices.Data;
using FinanceServices.Models.API;
using FinanceServices.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        bool dataIsCached = false;
        private static List<UsStock>? NasdaqList { get; set; }
        private static List<UsStock>? NyseList { get; set; }
        private static List<Crypto>? BinanceList { get; set; }

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
            if (dataIsCached == false)
            {
                //do initial API calls
                await InitializeCachedData();
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateMarketStatus(stoppingToken);
            }
        }
        private async Task InitializeCachedData()
        {
            try
            {
                var symbolList = FinanceConstants.LargeSymbolsList;
                NasdaqList = await GetNasdaqStockList(); //1 API call
                NyseList = await GetNyseStockList();    // 1 API call
                BinanceList = await GetBinanceCryptoList(); // 1 API call

                foreach (var symbol in symbolList)
                {
                    if (symbol.Contains("BINANCE"))
                    {
                        foreach (var item in BinanceList)
                        {
                            if (item.Symbol == symbol)
                            {
                                _cache.CryptoQuotes[symbol] = item;
                            }
                        }
                    }
                    else
                    {
                        var quote = await GetStockQuote(symbol); //symbolList size nr API calls
                        if (quote == null)
                            throw new Exception($"No stock quote for symbol: {symbol}");
                        _cache.StockQuotes[symbol] = quote;

                        //slow operation (loops through thousands of stocks)
                        var info = GetNasdaqStock(symbol) ?? GetNyseStock(symbol)
                            ?? throw new Exception($"No stock info for symbol: {symbol}");
                        _cache.UsStocks[symbol] = info;
                    }

                }
                dataIsCached = true;
            }
            catch (Exception ex)
            {
                dataIsCached = false;
                _logger.LogWarning($"{ex}, Failed to cache data at startup");
            }
        }

        private async Task UpdateMarketStatus(CancellationToken stoppingToken)
        {
            try
            {
                var marketStatus = await GetMarketStatus(FinanceConstants.US);
                _cache.MarketStatus[FinanceConstants.US] = marketStatus;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching market status: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
        private async Task<List<UsStock>> GetNasdaqStockList()
        {
            if (_apiCallsCounter.IsCallPossible())
            {
                string baseUrl = FinanceConstants.BaseUrl;
                string Nasdaq = "stock/symbol?exchange=US&mic=XNAS";
                var url = $"{baseUrl}{Nasdaq}&token={_apiKey}";
                var list = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
                return list;
            }
            else
            {
                throw new Exception("Api Calls Limit reached!");
            }
        }
        private async Task<List<UsStock>> GetNyseStockList()
        {
            if (_apiCallsCounter.IsCallPossible())
            {
                string baseUrl = FinanceConstants.BaseUrl;
                string Nyse = "stock/symbol?exchange=US&mic=XNYS";
                var url = $"{baseUrl}{Nyse}&token={_apiKey}";
                var list = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
                return list;
            }
            else
            {
                throw new Exception("Api Calls Limit reached!");
            }
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
                throw new Exception("Api Calls Limit reached!");
            }
        }
        private async Task<StockQuote> GetStockQuote(string tickerSymbol)
        {
            if (_apiCallsCounter.IsCallPossible())
            {
                string baseUrl = FinanceConstants.BaseUrl;
                string stockQuote = $"quote?symbol={tickerSymbol}";
                var url = $"{baseUrl}{stockQuote}&token={_apiKey}";
                var quote = await _httpClient.GetFromJsonAsync<StockQuote>(url);
                return quote;
            }
            else
            {
                throw new Exception("Api Calls Limit reached!");
            }
        }
        private UsStock GetNasdaqStock(string symbol)
        {
            if (NasdaqList == null)
                return null;

            foreach (var stock in NasdaqList)
            {
                if (stock.Symbol == symbol)
                    return stock;
            }
            return null;
        }
        private UsStock GetNyseStock(string symbol)
        {
            if (NyseList == null)
                return null;

            foreach (var stock in NyseList)
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
                throw new Exception("Api Calls Limit reached!");
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

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using YesterdayNews.Hubs;
using YesterdayNews.Models.API;

namespace YesterdayNews.Services
{
    public class FinnhubBackgroundService : BackgroundService
    {
        private readonly IHubContext<StockHub> _hubContext;
        private readonly ILogger<FinnhubBackgroundService> _logger;
        private readonly HttpClient _httpClient;

        private readonly string _apiKey;
        private readonly string _baseUrl = "https://finnhub.io/api/v1/";

        //Cached data
        bool dataIsCached = false;
        private readonly string[] symbolList = { "NVDA", "MSFT", "AAPL" };
        public static List<UsStock>? NasdaqList { get; private set; }
        public static List<UsStock>? NyseList { get; private set; }
        public static Dictionary<string, StockQuote> StockQuotes { get; private set; } = new();

        public FinnhubBackgroundService(IHubContext<StockHub> hubContext, HttpClient httpClient, IConfiguration config, ILogger<FinnhubBackgroundService> logger)
        {
            _hubContext = hubContext;
            _apiKey = "" + config["Finnhub:ApiKey"];
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Updates cached data with API calls and gets tradedata with a websocket realtime
        /// Max limit with Finnhub for free version is 50 symbols
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (dataIsCached == false)
            {
                //do initial API calls
                await InitializeCachedData(symbolList);
            }

            //Connect
            using var websocket = new ClientWebSocket(); //"using" keyword ensure its disposed automatically when out of scope
            var url = $"wss://ws.finnhub.io?token={_apiKey}";
            await websocket.ConnectAsync(new Uri(url), stoppingToken);

            //Subscribe-Send
            foreach (var sym in symbolList)
            {
                var msg = JsonSerializer.Serialize(new { type = "subscribe", symbol = sym });
                await websocket.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, stoppingToken);
            }

            var buffer = new byte[4096];
            while (!stoppingToken.IsCancellationRequested)
            {
                //Recieve
                var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    var response = JsonSerializer.Deserialize<TradesResponse>(json);
                    if (response?.Data != null && response.Data.Count > 0)
                    {
                        //store updated prices in cached StockQuotes
                        foreach (var trade in response.Data)
                        {
                            if (StockQuotes.ContainsKey(trade.Symbol))
                            {
                                StockQuotes[trade.Symbol].CurrentPrice = trade.Price;
                                StockQuotes[trade.Symbol].TimeStamp = trade.TimeStamp;
                            }
                        }

                        //transform naming from JSON property names to actual names (See StockQuote model)
                        var stockQuotesForClient = StockQuotes.ToDictionary(
                                    kvp => kvp.Key,
                                    kvp => new
                                    {
                                        CurrentPrice = kvp.Value.CurrentPrice,
                                        PercentageChange = kvp.Value.PercentageChange
                                    }
                        );
                        //Send tradedata to all clients
                        await _hubContext.Clients.All.SendAsync("ReceiveStockUpdates", stockQuotesForClient, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket parse error");
                }
            }
        }

        private async Task InitializeCachedData(string[] SymbolList)
        {
            try
            {
                NasdaqList = await GetNasdaqStockList(); //1 API call
                NyseList = await GetNyseStockList();    // 1 API call
                foreach (var symbol in SymbolList)
                {
                    var quote = await GetStockQuote(symbol); //symbolList size nr API calls
                    if (quote == null)
                        throw new Exception($"No data for symbol: {symbol}");

                    StockQuotes[symbol] = quote;
                }
                dataIsCached = true;
            }
            catch (Exception ex)
            {
                dataIsCached = false;
                _logger.LogError(ex, "Failed to cache data at startup");
            }
        }
        private async Task<List<UsStock>> GetNasdaqStockList()
        {
            if (NasdaqList != null) return NasdaqList;

            string Nasdaq = "stock/symbol?exchange=US&mic=XNAS";
            var url = $"{_baseUrl}{Nasdaq}&token={_apiKey}";
            NasdaqList = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
            return NasdaqList;
        }
        private async Task<List<UsStock>> GetNyseStockList()
        {
            if (NyseList != null) return NyseList;

            string Nyse = "stock/symbol?exchange=US&mic=XNYS";
            var url = $"{_baseUrl}{Nyse}&token={_apiKey}";
            NyseList = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
            return NyseList;
        }
        private async Task<StockQuote> GetStockQuote(string tickerSymbol)
        {
            string stockQuote = $"/quote?symbol={tickerSymbol}";
            var url = $"{_baseUrl}{stockQuote}&token={_apiKey}";
            var quote = await _httpClient.GetFromJsonAsync<StockQuote>(url);
            return quote;
        }
        public static StockQuote? GetCachedStockQuote(string symbol)
        {
            return StockQuotes.TryGetValue(symbol, out var quote) ? quote : null;
        }

        //public async Task<string> GetForexQuotes(string exchangeName)
        //{
        //    throw new NotImplementedException();
        //}
        //public async Task<string> GetCryptoQuotes(string exchangeName)
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
        //public async Task<string> GetMarketStatus(string exchangeName)
        //{
        //    throw new NotImplementedException();
        //}
        //public async Task<string> GetMarketNews(string marketType)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

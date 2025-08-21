using FinanceServices.Models.API;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace FinanceServices.Services.BackgroundServices
{
    public class FinnhubBackgroundService : BackgroundService
    {
        //private readonly IHubContext<FinanceHub> _hubContext;
        public event Action<Dictionary<string, object>> OnPriceUpdate;
        private readonly HttpClient _httpClient;

        private readonly string _apiKey;
        private readonly string _baseUrl = "https://finnhub.io/api/v1/";
        public static readonly string NYSE = "XNYS";
        public static readonly string NASDAQ = "XNAS";

        //Cached data
        bool dataIsCached = false;
        private static readonly string[] symbolsList = { "NVDA", "MSFT", "AAPL", "GOOG", "GOOGL", "AMZN", "META", "AVGO", "NFLX", "TSLA",
                    "BRK.B", "TSM", "V", "LLY", "MA", "JPM", "WMT", "ORCL", "XOM", "BRK.A",
                    "BINANCE:BTCUSDT", "BINANCE:ETHUSDT", "BINANCE:XRPUSDT", "BINANCE:BNBUSDT", "BINANCE:SOLUSDT", "BINANCE:DOGEUSDT", "BINANCE:TRXUSDT", "BINANCE:ADAUSDT","BINANCE:LINKUSDT", "BINANCE:HYPEUSDT" };
        private static List<UsStock>? NasdaqList { get; set; }
        private static List<UsStock>? NyseList { get; set; }
        private static List<Crypto>? BinanceList { get; set; }
        public static ConcurrentDictionary<string, StockQuote> StockQuotes { get; private set; } = new();
        public static ConcurrentDictionary<string, UsStock> UsStocks { get; private set; } = new();
        public static ConcurrentDictionary<string, Crypto> CryptoQuotes { get; private set; } = new();

        public FinnhubBackgroundService(/*IHubContext<FinanceHub> hubContext,*/ HttpClient httpClient, IConfiguration config)
        {
            //_hubContext = hubContext;
            _apiKey = "" + config["Finnhub:ApiKey"];
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (dataIsCached == false)
            {
                //do initial API calls
                await InitializeCachedData(symbolsList);
            }

            //Connect
            using var websocket = new ClientWebSocket(); //"using" keyword ensure its disposed automatically when out of scope
            var url = $"wss://ws.finnhub.io?token={_apiKey}";
            await websocket.ConnectAsync(new Uri(url), stoppingToken);

            //Subscribe-Send
            foreach (var sym in symbolsList)
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
                        //store updated prices in cached StockQuotes and CryptoQuotes
                        foreach (var trade in response.Data)
                        {
                            if (StockQuotes.ContainsKey(trade.Symbol))
                            {
                                var stock = StockQuotes[trade.Symbol];
                                stock.CurrentPrice = trade.Price;
                                stock.TimeStamp = trade.TimeStamp;
                            }
                            else if (CryptoQuotes.ContainsKey(trade.Symbol))
                            {
                                var crypto = CryptoQuotes[trade.Symbol];
                                crypto.CurrentPrice = trade.Price;
                                crypto.TimeStamp = trade.TimeStamp;
                            }
                        }
                        var updates = MergeStocksAndCryptos();
                        RaisePriceUpdate(updates);
                        //await _hubContext.Clients.All.SendAsync("ReceivePriceUpdates", updates);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex}, WebSocket parse error");
                }
            }
        }
        private void RaisePriceUpdate(Dictionary<string, object> updates)
        {
            OnPriceUpdate?.Invoke(updates);
        }

        private async Task InitializeCachedData(string[] SymbolList)
        {
            try
            {
                NasdaqList = await GetNasdaqStockList(); //1 API call
                NyseList = await GetNyseStockList();    // 1 API call
                BinanceList = await GetBinanceCryptoList(); // 1 API call
                foreach (var symbol in SymbolList)
                {
                    if (symbol.Contains("BINANCE"))
                    {
                        foreach (var item in BinanceList)
                        {
                            if (item.Symbol == symbol)
                            {
                                CryptoQuotes[symbol] = item;
                            }
                        }
                    }
                    else
                    {
                        var quote = await GetStockQuote(symbol); //symbolList size nr API calls
                        if (quote == null)
                            throw new Exception($"No stock quote for symbol: {symbol}");
                        StockQuotes[symbol] = quote;

                        //slow operation (loops through thousands of stocks)
                        var info = GetNasdaqStock(symbol) ?? GetNyseStock(symbol)
                            ?? throw new Exception($"No stock info for symbol: {symbol}");
                        UsStocks[symbol] = info;
                    }

                }
                dataIsCached = true;
            }
            catch (Exception ex)
            {
                dataIsCached = false;
                Console.WriteLine($"{ex}, Failed to cache data at startup");
            }
        }
        private async Task<List<UsStock>> GetNasdaqStockList()
        {
            if (NasdaqList != null) return NasdaqList;

            string Nasdaq = "stock/symbol?exchange=US&mic=XNAS";
            var url = $"{_baseUrl}{Nasdaq}&token={_apiKey}";
            var list = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
            return list;
        }
        private async Task<List<UsStock>> GetNyseStockList()
        {
            if (NyseList != null) return NyseList;

            string Nyse = "stock/symbol?exchange=US&mic=XNYS";
            var url = $"{_baseUrl}{Nyse}&token={_apiKey}";
            var list = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
            return list;
        }
        private async Task<List<Crypto>> GetBinanceCryptoList()
        {
            if (BinanceList != null) return BinanceList;

            string binance = "crypto/symbol?exchange=binance";
            string url = $"{_baseUrl}{binance}&token={_apiKey}";
            var list = await _httpClient.GetFromJsonAsync<List<Crypto>>(url) ?? new List<Crypto>();
            return list;
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
        public static UsStock? GetCachedUsStock(string symbol)
        {
            return UsStocks.TryGetValue(symbol, out var info) ? info : null;
        }
        public static Crypto? GetCachedCryptoQuote(string symbol)
        {
            return CryptoQuotes.TryGetValue(symbol, out var quote) ? quote : null;
        }
        private UsStock GetNasdaqStock(string symbol)
        {
            if(NasdaqList  == null) 
                return null;

            foreach(var stock in NasdaqList)
            {
                if(stock.Symbol == symbol) 
                    return stock;
            }
            return null;
        }
        private UsStock GetNyseStock(string symbol)
        {
            if(NyseList  == null) 
                return null;

            foreach(var stock in NyseList)
            {
                if(stock.Symbol == symbol) 
                    return stock;
            }
            return null;
        }
        private Dictionary<string, object> MergeStocksAndCryptos()
        {
            var updates = new Dictionary<string, object>();
            foreach (var stockQuote in StockQuotes)
            {
                updates[stockQuote.Key] = new
                {
                    stockQuote.Value.CurrentPrice,
                    stockQuote.Value.Change,
                    stockQuote.Value.PercentageChange,
                };
            }
            foreach (var crypto in CryptoQuotes)
            {
                updates[crypto.Key] = new
                {
                    crypto.Value.CurrentPrice,
                    crypto.Value.Change,
                    crypto.Value.PercentageChange,
                };
            }
            return updates;
        }
        //public async Task<string> GetMarketStatus(string exchangeName)
        //{
        //    throw new NotImplementedException();
        //}

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

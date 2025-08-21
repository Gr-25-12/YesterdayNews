using FinanceServices.Data;
using FinanceServices.Models.API;
using FinanceServices.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace FinanceServices.Services.BackgroundServices
{
    public class FinnhubWebSocketService : BackgroundService
    {

        public event Action<Dictionary<string, object>>? OnPriceUpdate;
        private readonly string _apiKey;
        private readonly MarketDataCache _cache;

        public FinnhubWebSocketService(IConfiguration config, MarketDataCache Cache)
        {
            _apiKey = "" + config["Finnhub:ApiKey"];
            _cache = Cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var symbolsList = FinanceConstants.LargeSymbolsList;

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
                            if (_cache.StockQuotes.ContainsKey(trade.Symbol))
                            {
                                var stock = _cache.StockQuotes[trade.Symbol];
                                stock.CurrentPrice = trade.Price;
                                stock.TimeStamp = trade.TimeStamp;
                            }
                            else if (_cache.CryptoQuotes.ContainsKey(trade.Symbol))
                            {
                                var crypto = _cache.CryptoQuotes[trade.Symbol];
                                crypto.CurrentPrice = trade.Price;
                                crypto.TimeStamp = trade.TimeStamp;
                            }
                        }
                        var updates = MergeStocksAndCryptos();
                        RaisePriceUpdate(updates);
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
        private Dictionary<string, object> MergeStocksAndCryptos()
        {
            var updates = new Dictionary<string, object>();
            foreach (var stockQuote in _cache.StockQuotes)
            {
                updates[stockQuote.Key] = new
                {
                    stockQuote.Value.CurrentPrice,
                    stockQuote.Value.Change,
                    stockQuote.Value.PercentageChange,
                };
            }
            foreach (var crypto in _cache.CryptoQuotes)
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
    }
}

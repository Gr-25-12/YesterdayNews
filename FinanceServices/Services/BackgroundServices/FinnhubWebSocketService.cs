using FinanceServices.Data;
using FinanceServices.Models.API;
using FinanceServices.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace FinanceServices.Services.BackgroundServices
{
    public class FinnhubWebSocketService : BackgroundService
    {

        public event Func<Task> OnPriceUpdate;
        private readonly string _apiKey;
        private readonly MarketDataCache _cache;
        private readonly ILogger<FinnhubWebSocketService> _logger;
        public FinnhubWebSocketService(IConfiguration config, MarketDataCache Cache, ILogger<FinnhubWebSocketService> logger)
        {
            _apiKey = "" + config["Finnhub:ApiKey"];
            _cache = Cache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var symbolsList = _cache.GetAllSymbols();
                if (!symbolsList.Any())
                {
                    _logger.LogInformation("No stocks or crypto in cache. Waiting 1 minute before retry...");
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogError("Task Cancelled in FinnhubWebSocketService!");
                        return;
                    }
                    continue;
                }
                using var websocket = new ClientWebSocket();
                var url = $"wss://ws.finnhub.io?token={_apiKey}";

                try
                {
                    _logger.LogInformation("Connecting to Finnhub WebSocket...");
                    await websocket.ConnectAsync(new Uri(url), stoppingToken);

                    SubscribeToWebsocket(symbolsList, websocket, stoppingToken);

                    var buffer = new byte[4096];

                    // Receive loop
                    while (!stoppingToken.IsCancellationRequested &&
                           websocket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult? result = null;

                        try
                        {
                            result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                        }
                        catch (TaskCanceledException)
                        {
                            _logger.LogInformation("Shutting down WebSocket (task canceled).");
                            return;
                        }
                        catch (WebSocketException wsex)
                        {
                            _logger.LogError(wsex, "WebSocket error");
                            break;
                        }

                        if (result?.MessageType == WebSocketMessageType.Close)
                        {
                            _logger.LogWarning("WebSocket closed by server");
                            break;
                        }

                        if (result != null)
                        {
                            await UpdateResult(result, buffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in WebSocket connection");
                }

                _logger.LogWarning("WebSocket disconnected. Reconnecting in 5 seconds...");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    return; // End if service is closed
                }
            }
        }
        private async void SubscribeToWebsocket(string[] symbolsList, ClientWebSocket websocket, CancellationToken stoppingToken)
        {
            foreach (var sym in symbolsList)
            {
                var msg = JsonSerializer.Serialize(new { type = "subscribe", symbol = sym });
                await websocket.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, stoppingToken);
                //_logger.LogInformation("Subscribed to {Symbol}", sym);
            }
        }
        private async Task UpdateResult(WebSocketReceiveResult result, byte[] buffer)
        {
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            try
            {
                var response = JsonSerializer.Deserialize<TradesResponse>(json);
                if (response?.Data != null && response.Data.Count > 0)
                {
                    UpdateCachedPrices(response.Data);
                    await RaisePriceUpdate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse WebSocket message: {Json}", json);
            }
        }
        private async Task RaisePriceUpdate()
        {
            await OnPriceUpdate?.Invoke();
        }
        private void UpdateCachedPrices(List<TradeData> data)
        {
            foreach (var trade in data)
            {
                if (_cache.Stocks.ContainsKey(trade.Symbol))
                {
                    var stock = _cache.Stocks[trade.Symbol];
                    stock.CurrentPrice = trade.Price;
                }
                else if (_cache.CryptoQuotes.ContainsKey(trade.Symbol))
                {
                    var crypto = _cache.CryptoQuotes[trade.Symbol];
                    crypto.CurrentPrice = trade.Price;
                    crypto.TimeStamp = trade.TimeStamp;
                }
                else if (_cache.Currencies.ContainsKey(trade.Symbol)) 
                {
                    
                    var forex = _cache.Currencies[trade.Symbol];
                    forex.CurrentPrice = trade.Price;
                    forex.TimeStamp = trade.TimeStamp;
                }
                else if (_cache.Commodities.ContainsKey(trade.Symbol))
                {
                    var forex = _cache.Commodities[trade.Symbol];
                    forex.CurrentPrice = trade.Price;
                    forex.TimeStamp = trade.TimeStamp;
                }
            }
        }
    }
}

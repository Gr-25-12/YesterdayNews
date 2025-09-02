using FinanceServices.Data;
using FinanceServices.Models.API;
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
            int retryDelay = 5;

            while (!stoppingToken.IsCancellationRequested)
            {
                var symbolsList = _cache.GetAllSymbols();
                if (!symbolsList.Any())
                {
                    _logger.LogInformation("No stocks or crypto in cache. retrying in 5s...");
                    await DelayWithCancellation(5, stoppingToken);
                    continue;
                }
                using var websocket = new ClientWebSocket();

                try
                {
                    await ConnectWebSocketAsync(websocket, stoppingToken);
                    retryDelay = 5;
                    await SubscribeToSymbols(symbolsList, websocket, stoppingToken);
                    await ReceiveLoop(websocket, stoppingToken);
                }
                catch (Exception ex)
                {
                    HandleWebSocketException(ex);
                }
                _logger.LogWarning("WebSocket disconnected. Reconnecting in {RetryDelay}s...", retryDelay);
                await DelayWithCancellation(retryDelay, stoppingToken);
                retryDelay = Math.Min(retryDelay * 2, 60);

            }
        }
        private async Task DelayWithCancellation(int seconds, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Closing down");
                // End if service is closed
            }
        }
        private async Task ConnectWebSocketAsync(ClientWebSocket websocket, CancellationToken stoppingToken)
        {
            var url = $"wss://ws.finnhub.io?token={_apiKey}";
            await websocket.ConnectAsync(new Uri(url), stoppingToken);
            if (websocket.State == WebSocketState.Open)
                _logger.LogInformation("WebSocket connection established.");
        }
        private async Task SubscribeToSymbols(string[] symbolsList, ClientWebSocket websocket, CancellationToken stoppingToken)
        {
            foreach (var sym in symbolsList)
            {
                var msg = JsonSerializer.Serialize(new { type = "subscribe", symbol = sym });
                await websocket.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, stoppingToken);
                //_logger.LogInformation("Subscribed to {Symbol}", sym);
            }
        }
        private async Task ReceiveLoop(ClientWebSocket websocket, CancellationToken token)
        {
            var buffer = new byte[4096];

            while (!token.IsCancellationRequested && websocket.State == WebSocketState.Open)
            {
                using var memoryStream = new MemoryStream();
                WebSocketReceiveResult? result;

                do
                {
                    result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                        throw new WebSocketException();

                    memoryStream.Write(buffer, 0, result.Count);

                } while (!result.EndOfMessage);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var message = memoryStream.ToArray();

                await UpdateResult(message);
            }
        }

        private async Task UpdateResult(byte[] message)
        {
            var json = Encoding.UTF8.GetString(message);
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

        private void HandleWebSocketException(Exception ex)
        {
            if (ex is TaskCanceledException)
            {
                _logger.LogWarning("WebSocket service task canceled.");
                return;
            }

            if (ex is WebSocketException wsex)
            {
                var msg = wsex.Message;
                if (msg.Contains("without completing the close handshake"))
                {
                    _logger.LogWarning("WebSocket closed by server without handshake (EOF).");
                }
                else if (msg.Contains("429"))
                {
                    _logger.LogError("Too many open WebSocket connections (HTTP 429).");
                }
                else
                {
                    _logger.LogError(wsex, "WebSocket unexpected error.");
                }
            }
            else
            {
                _logger.LogError(ex, "Unhandled exception in WebSocket service.");
            }
        }
    }
}

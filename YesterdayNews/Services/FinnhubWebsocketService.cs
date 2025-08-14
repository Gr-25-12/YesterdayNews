using NuGet.Packaging.Signing;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using YesterdayNews.Models.API;

namespace YesterdayNews.Services
{
    public class FinnhubWebsocketService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<FinnhubWebsocketService> _logger;

        public static Dictionary<string, StockQuote> LatestQuotes = new();
        public FinnhubWebsocketService(HttpClient httpClient, IConfiguration config, ILogger<FinnhubWebsocketService> logger)
        {
            _httpClient = httpClient;
            _apiKey = "" + config["Finnhub:ApiKey"];
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var websocket = new ClientWebSocket(); //"using" keyword ensure its disposed automatically when out of scope
            var url = $"wss://ws.finnhub.io?token={_apiKey}";
            await websocket.ConnectAsync(new Uri(url), stoppingToken);

            string[] symbols = { "NVDA", "MSFT", "AAPL" };
            //replace with GETSYMBOLS();
            foreach (var sym in symbols)
            {
                var msg = JsonSerializer.Serialize(new { type = "subscribe", symbol = sym });
                await websocket.SendAsync(Encoding.UTF8.GetBytes(msg), WebSocketMessageType.Text, true, stoppingToken);
            }

            var buffer = new byte[4096];
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    var response = JsonSerializer.Deserialize<TradesResponse>(json);
                    if (response?.Data != null)
                    {
                        foreach (var trade in response.Data)
                        {
                            LatestQuotes[trade.Symbol] = new StockQuote
                            {
                                CurrentPrice = trade.Price,
                                TimeStamp = trade.TimeStamp
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket parse error");
                }
            }
        }
    }
}

using FinanceServices.Data;
using FinanceServices.Models.API;
using FinanceServices.Utilities;
using Microsoft.AspNetCore.SignalR;
using YesterdayNews.Services;

namespace YesterdayNews.Hubs
{
    public class FinanceHub : Hub
    {
        //can be left empty
        //add methods here if clients need to call server, request something etc

        private static int _connectionCount = 0;
        private static readonly object _lock = new();
        private readonly MarketDataCache _cache;
        FinanceEventHandler _eventHandler;
        public FinanceHub(MarketDataCache dataCache, FinanceEventHandler eventHandler)
        {
            _cache = dataCache;
            _eventHandler = eventHandler;
        }
        public static int ConnectionCount
        {
            get { 
                lock (_lock) 
                { 
                    return _connectionCount; 
                } 
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            lock (_lock)
            {
                _connectionCount++; 
            }

            foreach (var symbol in FinanceConstants.LargeSymbolsList)
            {
                var stock = _cache.GetCachedStock(symbol);

                if (stock == null)
                {
                    //DO something? maybe oif stock not found our stock price is (error) / null
                    //await _eventHandler.HandleStockquoteApiError(symbol, "Stock info not found");
                    continue;
                }
            }

            var status = _cache.GetCachedMarketStatus(FinanceConstants.US);
            if (status == null)
                await _eventHandler.HandleMarketStatusApiError("US market status unavailable");
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_lock)
            {
                _connectionCount--;
            }
            return base.OnDisconnectedAsync(exception);
        }

    }
}

using FinanceServices.Data;
using FinanceServices.Models.API;
using FinanceServices.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using YesterdayNews.Hubs;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class FinanceEventHandler : IFinanceEventHandler
    {
        private readonly IHubContext<FinanceHub> _hubContext;
        private readonly ILogger<FinanceEventHandler> _logger;
        private readonly MarketDataCache _cache;

        public FinanceEventHandler(IHubContext<FinanceHub> hubContext, ILogger<FinanceEventHandler> logger, MarketDataCache cache)
        {
            _hubContext = hubContext;
            _logger = logger;
            _cache = cache;
        }

        public async Task HandlePriceUpdate()
        {
            try
            {
                var status = _cache.GetCachedMarketStatus(FinanceConstants.US);
                if (status != null  && status.Session != "Closed")
                {
                    //STOCKS
                    if (!_cache.Stocks.IsNullOrEmpty() )
                    {
                        await _hubContext.Clients.All.SendAsync("ReceivePriceUpdates", _cache.Stocks);
                    }         
                }
                //CRYPTO
                if (!_cache.CryptoQuotes.IsNullOrEmpty())
                {
                    await _hubContext.Clients.All.SendAsync("ReceivePriceUpdates", _cache.CryptoQuotes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Price Broadcast failed");
            }
        }

        public async Task HandleMarketStatusApiError(string error)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("NoMarketStatus", error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcast failed");
            }
        }

    }
}

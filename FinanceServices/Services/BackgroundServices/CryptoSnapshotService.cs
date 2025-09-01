using FinanceServices.Data;
using FinanceServices.Models.API;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;


namespace FinanceServices.Services.BackgroundServices
{
    public class CryptoSnapshotService : BackgroundService
    {

        private readonly MarketDataCache _cache;
        private ConcurrentDictionary<string, Crypto> Cryptos;
        private ConcurrentDictionary<string, Forex> Currencies;
        private ConcurrentDictionary<string, Forex> Commodities;
        private const int SNAPSHOT_INTERVAL = 60000; // 60000 ms = 1 minute
        public CryptoSnapshotService(MarketDataCache cache)
        {
            _cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Cryptos = _cache.CryptoQuotes;
                Currencies = _cache.Currencies;
                Commodities = _cache.Commodities;
                UpdateAllSnapshots();
                await Task.Delay(SNAPSHOT_INTERVAL, stoppingToken);
            }
        }

        private void UpdateAllSnapshots()
        {
            if (Cryptos != null)
            {
                foreach (var crypto in Cryptos.Values)
                {
                    crypto.UpdateSnapshots();
                }
            }
            if (Currencies != null)
            {
                foreach (var forex in Currencies.Values)
                {
                    forex.UpdateSnapshots();
                }
            }
            if (Commodities != null)
            {
                foreach (var forex in Commodities.Values)
                {
                    forex.UpdateSnapshots();
                }
            }
        }
    }
}

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
                if (Cryptos != null)
                {
                    UpdateAllSnapshots();
                }
                await Task.Delay(SNAPSHOT_INTERVAL, stoppingToken);
            }
        }

        private void UpdateAllSnapshots()
        {
            foreach (var crypto in Cryptos.Values)
            {
                crypto.UpdateSnapshots();
            }
        }
    }
}

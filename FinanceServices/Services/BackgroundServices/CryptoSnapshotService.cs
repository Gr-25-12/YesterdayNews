using System.Collections.Concurrent;
using FinanceServices.Models.API;
using Microsoft.Extensions.Hosting;


namespace FinanceServices.Services.BackgroundServices
{
    public class CryptoSnapshotService : BackgroundService
    {
        private ConcurrentDictionary<string, Crypto> Cryptos;
        private const int SNAPSHOT_INTERVAL = 60000; // 60000 ms = 1 minute

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Cryptos = FinnhubBackgroundService.CryptoQuotes;
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

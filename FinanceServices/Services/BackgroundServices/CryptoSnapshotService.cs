using Azure.Data.Tables;
using FinanceServices.Data;
using FinanceServices.Models.API;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;


namespace FinanceServices.Services.BackgroundServices
{
    public class CryptoSnapshotService : BackgroundService
    {

        private readonly MarketDataCache _cache;
        private readonly TableClient _tableClient;
        private ConcurrentDictionary<string, Crypto> Cryptos;
        private ConcurrentDictionary<string, Forex> Currencies;
        private ConcurrentDictionary<string, Forex> Commodities;

        private readonly string _snapshotKey = "Snapshots";
        private readonly string _snapshotPrice = "YesterdaysPrice";
        private const int SNAPSHOT_INTERVAL = 60000; // 60000 ms = 1 minute
        public CryptoSnapshotService(IConfiguration configuration, MarketDataCache cache)
        {
            _cache = cache;
            string connectionString = configuration["AzureBlobStorage"];
            string containerName = configuration["AzurePriceTable"];
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(containerName);
            _tableClient.CreateIfNotExists();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await _tableClient.UpsertEntityAsync(new TableEntity("Snapshots", "TEST") { { "YesterdaysPrice", 123.45m } });
            //run until they arent null
            while (!stoppingToken.IsCancellationRequested &&
                    (Cryptos == null || Currencies == null || Commodities == null))
            {
                Cryptos = _cache.CryptoQuotes;
                Currencies = _cache.Currencies;
                Commodities = _cache.Commodities;
                await Task.Delay(500, stoppingToken);
            }
            //Load once from the AzureTable
            if (Cryptos != null && Currencies != null && Commodities != null)
                await LoadSnapshotsFromTable();
            
            //Take a snapshot every minute
            while (!stoppingToken.IsCancellationRequested)
            {
                UpdateAllSnapshots();
                if (Commodities != null)
                {
                    foreach (var comm in Commodities.Values)
                    {
                        if (comm.Symbol != null && comm.Price24HoursAgo > 0)
                            await SaveSnapshotToTable(comm.Symbol, comm.Price24HoursAgo, DateTime.UtcNow);
                        else if (comm.Symbol != null)
                            await SaveSnapshotToTable(comm.Symbol, comm.CurrentPrice, DateTime.UtcNow);
                    }
                }
                await Task.Delay(SNAPSHOT_INTERVAL, stoppingToken);
            }
        }

        private async Task UpdateAllSnapshots()
        {
            if (Cryptos != null)
            {
                foreach (var crypto in Cryptos.Values)
                {
                    crypto.UpdateSnapshots();
                    if (crypto.Symbol != null && crypto.Price24HoursAgo > 0)
                        await SaveSnapshotToTable(crypto.Symbol, crypto.Price24HoursAgo, DateTime.UtcNow);

                }
            }
            if (Currencies != null)
            {
                foreach (var forex in Currencies.Values)
                {
                    forex.UpdateSnapshots();
                    if (forex.Symbol != null && forex.Price24HoursAgo > 0)
                        await SaveSnapshotToTable(forex.Symbol, forex.Price24HoursAgo, DateTime.UtcNow);
                }
            }
            if (Commodities != null)
            {
                foreach (var forex in Commodities.Values)
                {
                    forex.UpdateSnapshots();
                    if (forex.Symbol != null && forex.Price24HoursAgo > 0)
                        await SaveSnapshotToTable(forex.Symbol, forex.Price24HoursAgo, DateTime.UtcNow);
                }
            }
        }
        private async Task SaveSnapshotToTable(string symbol, decimal price, DateTime snapshotTime)
        {
            var entity = new TableEntity(_snapshotKey, symbol)
            {
                { _snapshotPrice, price },
                { "SnapshotTime", snapshotTime }
            };

            await _tableClient.UpsertEntityAsync(entity);
        }
        private async Task LoadSnapshotsFromTable()
        {
            foreach (var crypto in Cryptos.Values)
            {
                if (crypto.Symbol == null) continue;
                var tablePrice = await GetSnapshotAsync(crypto.Symbol);
                if (tablePrice.HasValue)
                    crypto.LoadSnapshotFromTable(tablePrice.Value);
            }
            foreach (var forex in Currencies.Values)
            {
                if (forex.Symbol == null) continue;
                var tablePrice = await GetSnapshotAsync(forex.Symbol);
                if (tablePrice.HasValue)
                    forex.LoadSnapshotFromTable(tablePrice.Value);
            }
            foreach (var comm in Commodities.Values)
            {
                if (comm.Symbol == null) continue;
                var tablePrice = await GetSnapshotAsync(comm.Symbol);
                if (tablePrice.HasValue)
                    comm.LoadSnapshotFromTable(tablePrice.Value);
            }
        }
        private async Task<decimal?> GetSnapshotAsync(string symbol)
        {
            try
            {
                var entity = await _tableClient.GetEntityAsync<TableEntity>(_snapshotKey, symbol);
                if (entity.Value.TryGetValue(_snapshotPrice, out object priceObj))
                {
                    return Convert.ToDecimal(priceObj);
                }
                return null;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Save all last cached prices to Azure Table before closing (might not be 24 hours then)
            if (Cryptos != null)
            {
                foreach (var crypto in Cryptos.Values)
                {
                    if (crypto.Symbol != null && crypto.Price24HoursAgo > 0)
                        await SaveSnapshotToTable(crypto.Symbol, crypto.Price24HoursAgo, DateTime.UtcNow);
                    else if(crypto.Symbol != null)
                        await SaveSnapshotToTable(crypto.Symbol, crypto.CurrentPrice, DateTime.UtcNow);
                }
            }
            if (Currencies != null)
            {
                foreach (var forex in Currencies.Values)
                {
                    if (forex.Symbol != null && forex.Price24HoursAgo > 0)
                        await SaveSnapshotToTable(forex.Symbol, forex.Price24HoursAgo, DateTime.UtcNow);
                    else if (forex.Symbol != null)
                        await SaveSnapshotToTable(forex.Symbol, forex.CurrentPrice, DateTime.UtcNow);
                }
            }
            if (Commodities != null)
            {
                foreach (var comm in Commodities.Values)
                {
                    if (comm.Symbol != null && comm.Price24HoursAgo > 0)
                        await SaveSnapshotToTable(comm.Symbol, comm.Price24HoursAgo, DateTime.UtcNow);
                    else if (comm.Symbol != null)
                        await SaveSnapshotToTable(comm.Symbol, comm.CurrentPrice, DateTime.UtcNow);
                }
            }

            await base.StopAsync(cancellationToken);
        }
    }
}

using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services.BackgroundServices
{
    public class WeatherApiBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WeatherApiBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(3);

        public WeatherApiBackgroundService(
            IServiceProvider serviceProvider,
             ILogger<WeatherApiBackgroundService> logger
            )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
           
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weather Background Service started");
            await RefreshPreloadedCitiesAsync();


            while (!stoppingToken.IsCancellationRequested) 
            {
                try
                {
                    await Task.Delay(_interval, stoppingToken);
                    //await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
                    await RefreshPreloadedCitiesAsync();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation( "background service forecast stopped");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occured whiel updating forecast data");
                }
            }
        }



        private async Task RefreshPreloadedCitiesAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherApiService>();
                await weatherService.RefreshPreloadedCitiesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduled weather forecast data update");
            }
        }
    }
}

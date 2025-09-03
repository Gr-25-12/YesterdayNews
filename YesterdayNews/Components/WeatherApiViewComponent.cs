using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using YesterdayNews.Models.ViewModels;

using YesterdayNews.Services.IServices;


namespace YesterdayNews.Components
{
    public class WeatherApiViewComponent : ViewComponent
    {
     
        private readonly IWeatherApiService _weatherApiService;
        private readonly IMemoryCache _cache;

        public WeatherApiViewComponent(IWeatherApiService weatherApiService,  IMemoryCache cache)
        {

            _weatherApiService = weatherApiService;
            _cache = cache;
        }
        public async Task<IViewComponentResult> InvokeAsync(double? lat = null, double? lon = null, string city = null)
        {
            try
            {
                List<ForecastVM> forecast;

                if (!string.IsNullOrEmpty(city))
                {
                    System.Console.WriteLine($"Getting weather for city: {city}");
                    forecast = await _weatherApiService.GetForecastByCityAsync(city);
                }
                else if (lat.HasValue && lon.HasValue)
                {
                    System.Console.WriteLine($"Getting weather for coordinates: {lat}, {lon}");
                    forecast = await _weatherApiService.GetForecastByCoordinatesAsync(lat.Value, lon.Value);
                }
                else
                {
                    forecast = await _weatherApiService.GetForecastByCityAsync(); // default city or geolocation
                }

                System.Console.WriteLine($"Got {forecast?.Count ?? 0} forecast items");
                return View(forecast ?? new List<ForecastVM>());
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error in WeatherApiViewComponent: {ex.Message}");
                return View(new List<ForecastVM>());
            }
        }

    }
}



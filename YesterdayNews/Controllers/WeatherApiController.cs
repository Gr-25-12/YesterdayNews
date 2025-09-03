
using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class WeatherApiController: Controller
    {
       
        private readonly IWeatherApiService _weatherApiService;
        public WeatherApiController(IWeatherApiService weatherApiService)
        {
          _weatherApiService = weatherApiService;
        }
    

        [HttpGet("/weather")]
        public async Task<IActionResult> Index(string city)

        {

            var forecast = string.IsNullOrWhiteSpace(city)
                ? await _weatherApiService.GetForecastByCityAsync()
                : await _weatherApiService.GetForecastByCityAsync(city);

            return View(forecast);
        }

        [HttpGet("/api/weather")]
        public Task<List<ForecastVM>> GetForecast()
            => _weatherApiService.GetForecastByCityAsync();

        [HttpGet("/api/weather/{city}")]
        public Task<List<ForecastVM>> GetForecast(string city)
            => _weatherApiService.GetForecastByCityAsync(city);

        [HttpGet("/api/weather/current")]
        public async Task<IActionResult> GetCurrentWeatherByCoordinates(double lat, double lon)
        {
            var forecast = await _weatherApiService.GetForecastByCoordinatesAsync(lat, lon);

            if (forecast == null || !forecast.Any())
                return NotFound();
            return Json(new
            {
                city = forecast.First().City,
                forecast = forecast
            });
        }



        // Internal component endpoint for widget updates (not a public API)
        [HttpGet("/Weather/component")]
        public async Task<IActionResult> WeatherComponent(double? lat, double? lon)
        {
            if (lat.HasValue && lon.HasValue)
            {
                return ViewComponent("WeatherApi", new { lat = lat.Value, lon = lon.Value });
            }

            return ViewComponent("WeatherApi");
        }



    }



}

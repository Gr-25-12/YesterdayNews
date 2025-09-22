
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
        public async Task<IActionResult> Index(double? lat, double? lon, string city)
        {
            if (!string.IsNullOrWhiteSpace(city))
            {
                var searchForecast = await _weatherApiService.GetMultiDayForecastByCityAsync(city);

                if (searchForecast == null)
                {
                    ViewBag.Message = $"No forecast found for \"{city}\". Please check the name and try again.";
                    return View(new List<ForecastVM>());
                }

                return View(searchForecast);
            }

            // Fallback default city if no lat/lon provided
            if (!lat.HasValue || !lon.HasValue)
            {
                city = "stockholm";
                var searchForecast = await _weatherApiService.GetMultiDayForecastByCityAsync(city);

                if (searchForecast == null)
                {
                    ViewBag.Message = $"No forecast found for \"{city}\". Please check the name and try again.";
                    return View(new List<ForecastVM>());
                }

                return View(searchForecast);
            }

            var forecast = await _weatherApiService.GetMultiDayForecastByCoordAsync(lat.Value, lon.Value);

            if (forecast == null)
            {
                ViewBag.Message = "Could not retrieve forecast for your location.";
                return View(new List<ForecastVM>());
            }

            return View(forecast);
        }



        [HttpGet("/Weather/component")]
        public async Task<IActionResult> WeatherComponent(double? lat, double? lon)
        {
            if (lat.HasValue && lon.HasValue)
            {
                return ViewComponent("WeatherApi", new { lat = lat.Value, lon = lon.Value });
            }


            return ViewComponent("WeatherApi", new { lat = 0.0, lon = 0.0 });
        }



    }



}

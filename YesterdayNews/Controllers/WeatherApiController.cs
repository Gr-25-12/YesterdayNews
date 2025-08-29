using Microsoft.AspNetCore.Http;
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




        public async Task<IActionResult> Index(string city)
        {
            var forecastdata = await _weatherApiService.GetForecastByCityAsync(city);

            if (forecastdata?.list == null || forecastdata.list.Length == 0)
            {
                ViewBag.Message = "No forecast data available. Please search for a city.";
                return View(new List<ForecastVM>());
            }

            var forecastProjection = forecastdata.list.Select(f =>
            {
                var weather = f.weather.FirstOrDefault();

                return new ForecastVM
                {
                    City = forecastdata.city.name,
                    Date = DateTime.Parse(f.dt_txt),
                    Summary = weather?.description,
                    TemperatureC = (int)f.main.temp,
                    IconUrl = weather != null ? $"http://openweathermap.org/img/wn/{weather.icon}@2x.png" : null
                };
            }).ToList();

            return View(forecastProjection);
        }

    }
}

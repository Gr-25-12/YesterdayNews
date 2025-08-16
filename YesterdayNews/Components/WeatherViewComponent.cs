using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.Api;
using YesterdayNews.Services.IServices;
using System.Threading.Tasks;



namespace YesterdayNews.Components
{
    public class WeatherViewComponent : ViewComponent
    {


        private readonly IWeatherApiService _weatherApiService;

        public WeatherViewComponent(IWeatherApiService weatherApiService)
        {
            _weatherApiService = weatherApiService;
        }


        public async Task<IViewComponentResult> InvokeAsync(string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                return View(null); 
            }

            var forecasts = await _weatherApiService.GetWeatherByCityAsync(city);

            var currentForecast = _weatherApiService.GetCurrentForecast(forecasts);

            return View(currentForecast);
        }

    }
}

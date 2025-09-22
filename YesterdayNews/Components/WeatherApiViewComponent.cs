using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using YesterdayNews.Models.ViewModels;

using YesterdayNews.Services.IServices;


namespace YesterdayNews.Components
{
    public class WeatherApiViewComponent : ViewComponent
    {
     
        private readonly IWeatherApiService _weatherApiService;
    

        public WeatherApiViewComponent(IWeatherApiService weatherApiService)
        {

            _weatherApiService = weatherApiService;
          
        }

        public async Task<IViewComponentResult> InvokeAsync(double lat, double lon)
        {
            var forecast = await _weatherApiService.GetSingleCurrentForecastByCoordAsync(lat, lon);
            return View(forecast); 
        }



    }
}



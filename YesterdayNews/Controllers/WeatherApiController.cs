using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

      
        public async  Task<IActionResult> Index()
        {
          var data = await _weatherApiService.GetWeatherAsync();
            return View(data);
        }
    }
}

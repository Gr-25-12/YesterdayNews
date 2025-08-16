using Microsoft.AspNetCore.Mvc;

namespace YesterdayNews.Components
{
    public class WeatherViewComponent : ViewComponent
    {



        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

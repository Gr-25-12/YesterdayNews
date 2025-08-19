using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models;
using YesterdayNews.Services;

namespace YesterdayNews.Controllers
{
    public class APINewsController : Controller
    {
        private readonly NewsService _newsService;

        public APINewsController(NewsService newsService)
        {
            _newsService = newsService; 
        }

        public async Task<IActionResult> TopNews()
        {
            var topNews = await _newsService.GetTopHeadlinesAsync("us");
            return View(topNews); 
        }
    }
}

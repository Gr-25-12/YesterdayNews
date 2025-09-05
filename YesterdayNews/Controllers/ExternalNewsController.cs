using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;

using YesterdayNews.Services;


namespace YesterdayNews.Controllers
{
    public class ExternalNewsController : Controller
    {
        private readonly ExternalNewsService _externalnews;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _environment;

        public ExternalNewsController(ExternalNewsService externalnews, IMemoryCache cache, IWebHostEnvironment environment)
        {
            _externalnews = externalnews;
            _cache = cache;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            if (!_cache.TryGetValue("TopNewsCache", out List<ExternalNewsVM> news))
            {
                news = await _externalnews.GetTopNewsAsync(_environment);
                _cache.Set("TopNewsCache", news, TimeSpan.FromHours(2));
            }

            return View(news);
        }
    }
}

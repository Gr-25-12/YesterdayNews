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
        public ExternalNewsController(ExternalNewsService externalnews, IMemoryCache cache)
        {
            _externalnews = externalnews;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            if (!_cache.TryGetValue("TopNewsCache", out List<ExternalNewsVM> news))
            {
                news = await _externalnews.GetTopNewsAsync();
                _cache.Set("TopNewsCache", news, TimeSpan.FromHours(2));
            }

            return View(news);
        }
    }
}

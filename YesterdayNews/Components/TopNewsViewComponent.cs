using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.Db;
using YesterdayNews.Services;

namespace YesterdayNews.ViewComponents
{
    public class TopNewsViewComponent : ViewComponent
    {
        private readonly NewsService _newsService;

        public TopNewsViewComponent(NewsService newsService)
        {
            _newsService = newsService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string country = "us", int maxArticles = 5)
        {
            var topNews = await _newsService.GetTopHeadlinesAsync(country, maxArticles);
            return View(topNews); // List<NewsArticle>
        }
    }
}

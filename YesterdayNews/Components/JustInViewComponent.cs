using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Components
{
    public class JustInViewComponent :ViewComponent
    {
        public readonly IArticleServices _articleServices;
        public JustInViewComponent(IArticleServices articleServices)
        {
            _articleServices = articleServices;
        }
        public IViewComponentResult Invoke(int categoryId = 0)
        {
            var articles = _articleServices.GetAllAsArticleVM(0,3, categoryId);
            var recentArticles = articles
                .Where(a => a.DateStamp >= DateTime.UtcNow.AddMinutes(-60))
                .ToList();
            return View(recentArticles);
        }
    }
}

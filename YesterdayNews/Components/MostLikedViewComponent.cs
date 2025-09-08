using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Components
{
    public class MostLikedViewComponent : ViewComponent
    {
        private readonly IArticleServices _articleServices;
        public MostLikedViewComponent(IArticleServices articleServices)
        {
            _articleServices = articleServices;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var articles = await _articleServices.GetMostLikedArticleVM(5);
            return View(articles);
        }
    }
}

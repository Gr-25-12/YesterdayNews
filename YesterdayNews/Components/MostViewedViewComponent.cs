using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Components
{
    public class MostViewedViewComponent : ViewComponent
    {
        private readonly IArticleServices _articleServices;
        public MostViewedViewComponent(IArticleServices articleServices)
        {
            _articleServices = articleServices;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            
            var articles = await _articleServices.GetMostViewedArticleVM(5);
            return View(articles);
        }
    }
}

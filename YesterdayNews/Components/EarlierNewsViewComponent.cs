using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Components
{
    public class EarlierNewsViewComponent :ViewComponent
    {
        private readonly IArticleServices _articleServices;

        public EarlierNewsViewComponent(IArticleServices articleServices)
        {
            _articleServices = articleServices;
        }
        public async Task<IViewComponentResult> InvokeAsync(int articlesToSkip, int categoryId = 0)
        {
            //skip number in articlesToSkip then get next 10 articles
            var articles = await _articleServices.GetAllPublishedByCategoryAsArticleVM(articlesToSkip, 10, categoryId);
            return View(articles);
        }
    }
    
}

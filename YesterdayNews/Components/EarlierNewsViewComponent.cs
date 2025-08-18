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
        public IViewComponentResult Invoke(int articlesToSkip, int categoryId = 0)
        {
            //skip number in articlesToSkip then get next 10 articles
            var articles = _articleServices.GetAllAsArticleVM(articlesToSkip, 10, categoryId);
            return View(articles);
        }
    }
    
}

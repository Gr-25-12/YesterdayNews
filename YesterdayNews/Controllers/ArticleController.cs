using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleServices _articleServices;

        public ArticleController(IArticleServices articleServices)
        {
            _articleServices = articleServices;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.Db;
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
            List<Article> articles = _articleServices.GetAll();
            return View(articles);
        }
        public IActionResult Details(int id)
        {
            var article = _articleServices.GetById(id);
            return View(article);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            
                IEnumerable<Article> articles = _articleServices
                    .GetAll();


                switch (status)
                {
                    case "draft":
                        articles = articles.Where(u => u.ArticleStatus == ArticleStatus.Draft); break;
                    case "pendingReview":
                        articles = articles.Where(u => u.ArticleStatus == ArticleStatus.PendingReview); break;
                    case "rejected":
                        articles = articles.Where(u => u.ArticleStatus == ArticleStatus.Rejected); break;
                    case "published":
                        articles = articles.Where(u => u.ArticleStatus == ArticleStatus.Published); break;
                    case "archived":
                        articles = articles.Where(u => u.ArticleStatus == ArticleStatus.Archived); break;
                    default: break;
                }

                var result = articles.Select(a => new
                {
                    id = a.Id,
                    headline = a.Headline,
                    author = new
                    {
                        firstName = a.Author?.FirstName ,
                        lastName = a.Author?.LastName
                    },
                    dateStamp = a.DateStamp,
                    category = new
                    {
                        name = a.Category?.Name
                    },
                    articleStatus = a.ArticleStatus.ToString(),
                    views = a.Views,
                    likes = a.Likes
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = result
                });
            
            
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                var articleToDelete = _articleServices.GetOne(id);
                if (articleToDelete == null)
                {
                    TempData["error"] = "Article not found!";
                    return Json(new { success = false, message = "Article not found!" });
                }

                _articleServices.Delete(id);
                TempData["success"] = "Article deleted successfully!";
                return Json(new { success = true, message = "Deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error while deleting: {ex.Message}"
                });
            }
        }
        #endregion
    }
}

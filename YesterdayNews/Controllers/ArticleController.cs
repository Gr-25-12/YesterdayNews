using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var articles = _articleServices
                    .GetAll()
                    .Select(a => new
                    {
                        id = a.Id,
                        headline = a.Headline,
                        author = new
                        {
                            firstName = a.Author?.FirstName ?? "Unknown",
                            lastName = a.Author?.LastName ?? ""
                        },
                        dateStamp = a.DateStamp.ToString("o"), // ISO 8601 format
                        category = new
                        {
                            name = a.Category?.Name ?? "Uncategorized"
                        },
                        articleStatus = a.ArticleStatus.ToString(),
                        views = a.Views,
                        likes = a.Likes
                    }).ToList();

                return Json(new
                {
                    success = true,
                    data = articles
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
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
        public IActionResult Create()
        {

            ViewBag.CategoryId = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "World" }
            }, "Value", "Text");

            ViewBag.AuthorId = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Test Author" }
            }, "Value", "Text");
            return View();
        }
        [HttpPost]
        public IActionResult Create(Article article)
        {
            try
            {
                UserManager<User>.
                //add USerService from Roberts code?
                article.DateStamp = DateTime.Now;
                article.ArticleStatus = ArticleStatus.Draft;
                article.Category = new Category() { Id = article.CategoryId, Name = "World" };
                article.Author = user;
                
                if (ModelState.IsValid)
                {
                    _articleServices.Add(article);
                    return RedirectToAction("Index");
                }
                return View(article);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error while creating article: {ex.Message}"
                });
            }
        }
        #endregion
    }
}

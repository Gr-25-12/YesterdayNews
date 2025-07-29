using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleServices _articleServices;
        private readonly ApplicationDbContext _context;

        public ArticleController(IArticleServices articleServices, ApplicationDbContext context)
        {
            _articleServices = articleServices;
            _context = context;
        }


        public IActionResult Index()
        {
            List<Article> articles = _articleServices.GetAll();
            return View(articles);
        }

        // GET: Article/Edit/5
        public IActionResult Edit(int id)
        {
            var article = _articleServices.GetOne(id);
            if (article == null)
            {
                return NotFound();
            }

            // Fetch categories and authors
            var categories = _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();
            var authors = _context.Users
                .Select(u => new SelectListItem { Value = u.Id, Text = u.FirstName + " " + u.LastName })
                .ToList();

            ViewBag.CategoryList = categories;
            ViewBag.AuthorList = authors;

            // for the dropdown
            ViewBag.ArticleStatusList = Enum.GetValues(typeof(ArticleStatus))
                .Cast<ArticleStatus>()
                .Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                }).ToList();

            return View(article);
        }

        // POST: Article/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Article article)
        {
            if (!ModelState.IsValid)
            {
                return View(article);
            }

            var existing = _articleServices.GetOne(article.Id);
            if (existing == null)
            {
                return NotFound();
            }

            // Update values
            existing.Headline = article.Headline;
            existing.Content = article.Content;
            existing.ContentSummary = article.ContentSummary;
            existing.CategoryId = article.CategoryId;
            existing.ArticleStatus = article.ArticleStatus;
            existing.ImageLink = article.ImageLink;

            // Save to DB
            _articleServices.Update(existing); // make sure you add this method

            TempData["success"] = "Article updated successfully!";
            return RedirectToAction("Index");
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
        #endregion
    }
}
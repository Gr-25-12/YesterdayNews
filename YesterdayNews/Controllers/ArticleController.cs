using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleServices _articleServices;
        private readonly UserManager<IdentityUser> _userManager;

        public ArticleController(IArticleServices articleServices, UserManager<IdentityUser> userManager)
        {
            _articleServices = articleServices;
            _userManager = userManager;
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
        public async Task<IActionResult> Create()
        {

            PopulateDropdownList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Article article, string action)
        {
            try
            {
                if (article.CategoryId == 0)
                {
                    ModelState.AddModelError("", "You must choose a category");
                }
                article.DateStamp = DateTime.Now;
                if (action == "draft")
                    article.ArticleStatus = ArticleStatus.Draft;
                else if (action == "review")
                    article.ArticleStatus = ArticleStatus.PendingReview;
                else if (action == "publish")
                    article.ArticleStatus = ArticleStatus.Published;

                article.Category = _articleServices.GetCategory(article.CategoryId);
                article.AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                article.Author = (User) await _userManager.FindByIdAsync(article.AuthorId);
                ModelState.Remove("Author");
                ModelState.Remove("AuthorId");
                ModelState.Remove("Category");
                if (ModelState.IsValid)
                {
                    _articleServices.Add(article);
                    return RedirectToAction("Index");
                }

                PopulateDropdownList();
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

        private void PopulateDropdownList()
        {
            var categories = _articleServices.GetAllCategories();
            categories.Insert(0, new Category { Id = 0, Name = "-- Choose Category --" });
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
        }
    }
}

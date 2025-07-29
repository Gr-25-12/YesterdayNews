using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleServices _articleServices;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IFileServices _fileServices;

        public ArticleController(IArticleServices articleServices, UserManager<IdentityUser> userManager, IFileServices fileServices)
        {
            _articleServices = articleServices;
            _userManager = userManager;
            _fileServices = fileServices;
        }


        public IActionResult Index()
        {
            List<Article> articles = _articleServices.GetAll();
            return View(articles);
        }
        public IActionResult Details(int id)
       {
            try
            {
            var article = _articleServices.GetById(id);

            return View(article);
            }
            catch (Exception ex)
            {

                TempData["error"] = $"{ex.Message}";
                throw;
            }
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
                TempData["error"] = "couldnt update";
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
            existing.AuthorId = article.AuthorId;// This was missing
            // Save to DB
            _articleServices.Edit(existing);

            TempData["success"] = "Article updated successfully!";
            return RedirectToAction("Index");
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
        public IActionResult Create()
        {
            PopulateDropdownList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Article article, IFormFile file, string action)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    var imageUrl = await _fileServices.UploadFileToContainer(file);
                    article.ImageLink = imageUrl;
                }

                if (article.CategoryId == 0)
                {
                    ModelState.AddModelError("CategoryId", "You must choose a category");
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
                
                if(article != null && IsArticleValid(article) )
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

        private bool IsArticleValid(Article article)
        {
            if(article.Author != null)
            {
                ModelState.Remove("Author");
                ModelState.Remove("AuthorId");
            }
            if (article.Category != null)
            {
                ModelState.Remove("Category");
            }
            
            if (article.ImageLink != null)
            {
                ModelState.Remove("ImageLink");
                ModelState.Remove("ImageFile");
            }
            else
            {
                ModelState.AddModelError("ImageLink", "You must upload an Image");
            }
            if (ModelState.IsValid)
            {
                return true;
            }
            return false;
        }
    }
}
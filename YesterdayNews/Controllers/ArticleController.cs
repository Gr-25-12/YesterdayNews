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
        private readonly ICategoryService _categoryService;

        public ArticleController(IArticleServices articleServices, UserManager<IdentityUser> userManager, IFileServices fileServices, ICategoryService categoryService)
        {
            _articleServices = articleServices;
            _userManager = userManager;
            _fileServices = fileServices;
            _categoryService = categoryService;
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
                    firstName = a.Author?.FirstName,
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var articleToDelete = _articleServices.GetOne(id);
                if (articleToDelete == null)
                {
                    TempData["error"] = "Article not found!";
                    return Json(new { success = false, message = "Article not found!" });
                }


                if (!string.IsNullOrEmpty(articleToDelete.ImageLink))
                {
                    await _fileServices.DeleteFileFromContainer(articleToDelete.ImageLink);
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
            PopulateCategoryDropdownList(null);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Article article, IFormFile imageFile, string action)
        {
            try
            {
                string newImageLink = await UploadImage(article, imageFile);
                if (newImageLink != null)
                    article.ImageLink = newImageLink;

                article.DateStamp = DateTime.Now;
                if (action == "draft")
                    article.ArticleStatus = ArticleStatus.Draft;
                else if (action == "review")
                    article.ArticleStatus = ArticleStatus.PendingReview;
                else if (action == "publish")
                    article.ArticleStatus = ArticleStatus.Published;

                article.Category = _categoryService.GetOne(article.CategoryId);
                article.AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                article.Author = (User)await _userManager.FindByIdAsync(article.AuthorId);

                if (article != null && IsArticleValid(article))
                {
                    _articleServices.Add(article);
                    return RedirectToAction("Index");
                }

                PopulateCategoryDropdownList(article);
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
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var article = _articleServices.GetById(id);
            if (article == null)
            {
                return NotFound();
            }
            // Fetch categories
            PopulateCategoryDropdownList(article);

            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> Edit(Article article, IFormFile imageFile)
        {
            var existing = _articleServices.GetById(article.Id);
            if (existing == null)
            {
                return NotFound();
            }

            // Update values
            existing.Headline = article.Headline;
            existing.Content = article.Content;
            existing.ContentSummary = article.ContentSummary;
            existing.CategoryId = article.CategoryId;
            existing.AuthorId = article.AuthorId;
            string newImageLink = await UploadImage(article, imageFile);
            if (newImageLink != null)
            {
                _fileServices.DeleteFileFromContainer(existing.ImageLink);
                existing.ImageLink = newImageLink;
            }
                

            if (article.ArticleStatus == ArticleStatus.Published)
                existing.ArticleStatus = ArticleStatus.PendingReview;
            else
                existing.ArticleStatus = article.ArticleStatus;

            if (IsArticleValid(existing) == false)
            {
                PopulateCategoryDropdownList(existing);
                return View(existing);
            }
            // Save to DB
            _articleServices.Edit(existing);
            TempData["success"] = "Article updated successfully!";
            return RedirectToAction("Index");
        }

        #endregion

        private void PopulateCategoryDropdownList(Article article)
        {
            var categories = _categoryService.GetAll();
            categories.Insert(0, new Category { Id = 0, Name = "-- Choose Category --" });
            int selectedId = article?.CategoryId ?? 0;
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", selectedId);   
        }

        private async Task<string> UploadImage(Article article, IFormFile imageFile)
        {
            if (article != null && imageFile != null && imageFile.Length > 0)
            {
                var imageUrl = await _fileServices.UploadFileToContainer(imageFile);
                return imageUrl;
            }
            return null;
        }

        private bool IsArticleValid(Article article)
        {
            if (article.Author != null)
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
            if (article?.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "You must choose a category");
            }
            if (ModelState.IsValid)
            {
                return true;
            }
            return false;
        }
    }
}
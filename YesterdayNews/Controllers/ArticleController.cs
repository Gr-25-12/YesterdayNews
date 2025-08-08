using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Net;
using System.Security.Claims;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;

namespace YesterdayNews.Controllers
{
    [Authorize(Roles = StaticConsts.Role_Admin + "," + StaticConsts.Role_Editor + "," + StaticConsts.Role_Journalist)]

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
            
                var userId = _userManager.GetUserId(User);
                var article = _articleServices.GetById(id, userId);

                if (article == null)
                {
                    TempData["error"] = "Article not found";
                return RedirectToAction("Index");
                }

            return View(article);
            
            
        }

        [HttpPost]
        public IActionResult ToggleLike(int id)
        {
            
                if (!User.Identity.IsAuthenticated)
                {
                    TempData["error"] = "You must be logged in to like articles";
                    return RedirectToAction("Details", new { id });
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["error"] = "User not found";
                    return RedirectToAction("Details", new { id });
                }

                var result = _articleServices.ToggleLike(userId, id);
                TempData["success"] = result ? "Article liked!" : "Article unliked!";

                return RedirectToAction("Details", new { id });
            
           
        }

     
        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {

            IEnumerable<Article> articles = _articleServices
                .GetAll();

            if (User.IsInRole(StaticConsts.Role_Journalist))
            {
                articles = articles.Where(a => a.AuthorId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

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
                var articleToDelete = _articleServices.GetById(id);
                if (articleToDelete == null)
                {
                    TempData["error"] = "Article not found!";
                    return Json(new { success = false, message = "Article not found!" });
                }

                if (User.IsInRole(StaticConsts.Role_Journalist))
                {

                    bool isDeletableStatus = articleToDelete.ArticleStatus.ToString() == StaticConsts.ArticleDraft ||
                                    articleToDelete.ArticleStatus.ToString() == StaticConsts.ArticleRejected;

                    if (!isDeletableStatus)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Journalists can only delete Draft or Rejected articles!"
                        });
                    }

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

                article.DateStamp = DateTime.UtcNow;
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
                    TempData["success"] = "Article created";
                    return RedirectToAction("Index");
                }

                PopulateCategoryDropdownList(article);
                return View(article);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Article could not created! {ex.Message}";

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
                

            if (article.ArticleStatus == ArticleStatus.Published || article.ArticleStatus == ArticleStatus.Rejected)
            {

                existing.ArticleStatus = ArticleStatus.PendingReview;
                existing.RejectionReason = null;
            }
            else
            {

                existing.ArticleStatus = article.ArticleStatus;
            }

            if (IsArticleValid(existing) == false)
            {
                PopulateCategoryDropdownList(existing);
                TempData["error"] = "Error occurred!";

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

        [HttpPost]
        [Authorize(Roles = StaticConsts.Role_Editor + "," + StaticConsts.Role_Admin + "," + StaticConsts.Role_Journalist)]
        public IActionResult SaveAsDraft(int id)
        {
            try
            {
                var article = _articleServices.GetById(id);
                if (article == null) return NotFound();

                article.ArticleStatus = ArticleStatus.Draft;
                _articleServices.Edit(article);

                TempData["success"] = "Article saved as draft successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                TempData["error"] = "Error saving as draft";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [Authorize(Roles = StaticConsts.Role_Journalist+ "," + StaticConsts.Role_Admin)]
        public IActionResult SubmitForReview(int id)
        {
            try
            {
                var article = _articleServices.GetById(id);
                if (article == null) return NotFound();

                article.ArticleStatus = ArticleStatus.PendingReview;
                _articleServices.Edit(article);

                TempData["success"] = "Article submitted for review";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                TempData["error"] = "Error submitting for review";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [Authorize(Roles = StaticConsts.Role_Editor + "," + StaticConsts.Role_Admin)]
        public IActionResult Publish(int id)
        {
            try
            {
                var article = _articleServices.GetById(id);
                if (article == null) return NotFound();

                article.ArticleStatus = ArticleStatus.Published;
                article.DateStamp = DateTime.UtcNow;
                _articleServices.Edit(article);

                TempData["success"] = "Article published successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                TempData["error"] = "Error publishing article";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [Authorize(Roles = StaticConsts.Role_Editor + "," + StaticConsts.Role_Admin)]
        public IActionResult Reject(int id,string rejectionReason)
        {
            try
            {
                var article = _articleServices.GetById(id);
                if (article == null) return NotFound();

                article.ArticleStatus = ArticleStatus.Rejected;
                article.RejectionReason = rejectionReason;
                _articleServices.Edit(article);

                TempData["success"] = "Article rejected";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                TempData["error"] = "Error rejecting article";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [Authorize(Roles = StaticConsts.Role_Editor + "," + StaticConsts.Role_Admin)]
        public IActionResult Archive(int id)
        {
            try
            {
                var article = _articleServices.GetById(id);
                if (article == null) return NotFound();

                article.ArticleStatus = ArticleStatus.Archived;
                _articleServices.Edit(article);

                TempData["success"] = "Article archived";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch
            {
                TempData["error"] = "Error archiving article";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
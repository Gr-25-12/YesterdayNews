using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;

namespace YesterdayNews.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IArticleServices _articleServices;
        private readonly ILikeService _likeServices;
        private readonly ISubscriptionServices _subscriptionServices;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly NewsService _newsService; // <-- inject NewsService

        public HomeController(
            ILogger<HomeController> logger,
            IArticleServices articleServices,
            ILikeService likeServices,
            UserManager<IdentityUser> userManager,
            ISubscriptionServices subscriptionServices,
            NewsService newsService) // <-- added here
        {
            _logger = logger;
            _articleServices = articleServices;
            _likeServices = likeServices;
            _userManager = userManager;
            _subscriptionServices = subscriptionServices;
            _newsService = newsService; // <-- assign
        }

        public async Task<IActionResult> Index(int categoryId = 0)
        {
            var latest = _articleServices.GetAllAsArticleVM(0, 3, categoryId); // List<ArticleVM>
            ViewData["SelectedCategory"] = categoryId;

            // Fetch top news from NewsAPI asynchronously
            var apiNews = await _newsService.GetTopHeadlinesAsync("us");

            var model = new HomeIndexVM
            {
                LatestArticles = latest,
                ApiNews = apiNews
            };

            return View(model);
        }

        public IActionResult Details(int id)
        {
            var article = _articleServices.GetById(id);
            if (article == null) return NotFound();

            string cookieName = $"ArticleView_{id}";
            if (!Request.Cookies.ContainsKey(cookieName))
            {
                _articleServices.IncrementViews(id);
                Response.Cookies.Append(cookieName, "Viewed", new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(StaticConsts.Cookie_Expires_IN),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
            }

            var userId = _userManager.GetUserId(User);
            article.IsLikedByCurrentUser = _articleServices.IsArticleLikedByUser(article, userId);

            bool hasAccess = true;
            if (User.IsInRole(StaticConsts.Role_Customer))
            {
                hasAccess = _subscriptionServices.HasActiveSubscription(userId);
            }

            ViewBag.HasAccess = hasAccess;
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

            var result = _likeServices.ToggleLike(userId, id);
            TempData["success"] = result ? "Article liked!" : "Article unliked!";
            return RedirectToAction("Details", new { id });
        }

        public IActionResult Privacy() => View();

        public IActionResult Search(string query)
        {
            var results = _articleServices.GetAllAsArticleVM(query);
            return View(results);
        }
    }
}

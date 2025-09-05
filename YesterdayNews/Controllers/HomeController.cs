using FinanceServices.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using YesterdayNews.Data;
using YesterdayNews.Models;
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;

namespace YesterdayNews.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IArticleServices _articleServices;
    private readonly ILikeService _likeServices;
    private readonly IFinanceApiServices _financeApiServices;
    private readonly ISubscriptionServices _subscriptionServices;
    private readonly UserManager<IdentityUser> _userManager;

    public ApplicationDbContext _db { get; }

    public HomeController(ILogger<HomeController> logger, IArticleServices articleServices, ILikeService likeServices, IFinanceApiServices financeApiServices ,UserManager<IdentityUser> userManager, ISubscriptionServices subscriptionServices ,ApplicationDbContext db)
    {
        _logger = logger;
        _articleServices = articleServices;
        _likeServices = likeServices;
        _financeApiServices = financeApiServices;
        _userManager = userManager;
        _subscriptionServices = subscriptionServices;
        _db = db;
    }

    public IActionResult Index(int categoryId = 0 , bool? adminView = null)
    {
        var latest = _articleServices.GetAllAsArticleVM(0, 6, categoryId);
        ViewData["SelectedCategory"] = categoryId;

        bool isAdminView = adminView ?? StaticConsts.AdminView;
        var isStaff = (User.IsInRole(StaticConsts.Role_Admin) || User.IsInRole(StaticConsts.Role_Editor) || User.IsInRole(StaticConsts.Role_Journalist));

        if (!isStaff || isAdminView == true)
        {
        return View(latest);

        }
        else
        {
            return RedirectToAction(nameof(AdminView));
        }
    }

  
    public IActionResult Details(int id)
    {
        var article = _articleServices.GetById(id);
        if (article == null)
            return NotFound();

        // Check if view cookie exists
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
            return RedirectToAction("Details",  new { id });
        }

        var result = _likeServices.ToggleLike(userId, id);
        TempData["success"] = result ? "Article liked!" : "Article unliked!";

        return RedirectToAction("Details",  new { id });


    }
    public IActionResult Markets()
    {
        var models = _financeApiServices.GetMarketsModel();
        return View(models);
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Search(string query)
    {
        var results = _articleServices.GetAllAsArticleVM(query);
        return View(results);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    #region AdminView 
    [Authorize(Roles = StaticConsts.Role_Admin + "," + StaticConsts.Role_Editor + "," + StaticConsts.Role_Journalist)]
    public IActionResult AdminView()
    {
        var dashboardData = GetDashboardData();
        StaticConsts.AdminView = false;

        return View(dashboardData);
    }

    [Authorize(Roles = StaticConsts.Role_Admin + "," + StaticConsts.Role_Editor + "," + StaticConsts.Role_Journalist)]
    public IActionResult SwitchToCustomerView(int categoryId = 0)
    {
        var latest = _articleServices.GetAllAsArticleVM(0, 6, categoryId);
        ViewData["SelectedCategory"] = categoryId;
        StaticConsts.AdminView = true;

        return View("Index", latest);
    }

    private AdminDashboardViewModel GetDashboardData()
    {
        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddDays(-30);
        var sixMonthsAgo = now.AddMonths(-6);

        var viewModel = new AdminDashboardViewModel();

        // Article Statistics
        var articles = _articleServices.GetAll();
        viewModel.TotalArticles = articles.Count;
        viewModel.PublishedArticles = articles.Count(a => a.ArticleStatus == ArticleStatus.Published);
        viewModel.DraftArticles = articles.Count(a => a.ArticleStatus == ArticleStatus.Draft);
        viewModel.PendingReviewArticles = articles.Count(a => a.ArticleStatus == ArticleStatus.PendingReview);
        viewModel.RejectedArticles = articles.Count(a => a.ArticleStatus == ArticleStatus.Rejected);
        viewModel.ArchivedArticles = articles.Count(a => a.ArticleStatus == ArticleStatus.Archived);

        // User Statistics
        var users = _userManager.Users.OfType<User>().ToList();
        var userRoles = _db.UserRoles.ToList();
        var roles = _db.Roles.ToList();

        viewModel.TotalUsers = users.Count;
        viewModel.CustomersCount = GetUserCountByRole(users, userRoles, roles, StaticConsts.Role_Customer);
        viewModel.JournalistsCount = GetUserCountByRole(users, userRoles, roles, StaticConsts.Role_Journalist);
        viewModel.EditorsCount = GetUserCountByRole(users, userRoles, roles, StaticConsts.Role_Editor);
        viewModel.AdminsCount = GetUserCountByRole(users, userRoles, roles, StaticConsts.Role_Admin);
        viewModel.ActiveUsers = users.Count(u => u.LockoutEnd == null || u.LockoutEnd <= now);
        viewModel.LockedUsers = users.Count(u => u.LockoutEnd != null && u.LockoutEnd > now);

        // Subscription Statistics
        var subscriptions = _subscriptionServices.GetAll();

        viewModel.TotalSubscriptions = subscriptions.Count;
        viewModel.ActiveSubscriptions = subscriptions.Count(s => !s.IsDeleted && (s.Expires == null || s.Expires > now));
        viewModel.ExpiredSubscriptions = subscriptions.Count(s => s.Expires != null && s.Expires <= now);

        // Revenue Statistics
        viewModel.TotalRevenue = subscriptions.Sum(s => s.SubscriptionType.Price);
        viewModel.RevenueLastWeek = subscriptions.Where(s => s.Created >= weekAgo).Sum(s => s.SubscriptionType.Price);
        viewModel.RevenueLastMonth = subscriptions.Where(s => s.Created >= monthAgo).Sum(s => s.SubscriptionType.Price);
        viewModel.RevenueLast6Months = subscriptions.Where(s => s.Created >= sixMonthsAgo).Sum(s => s.SubscriptionType.Price);

        // Subscription Counts by Time Period
        viewModel.SubscriptionsLast7Days = subscriptions.Count(s => s.Created >= weekAgo);
        viewModel.SubscriptionsLast30Days = subscriptions.Count(s => s.Created >= monthAgo);
        viewModel.SubscriptionsLast6Months = subscriptions.Count(s => s.Created >= sixMonthsAgo);

        // Chart Data
        viewModel.SubscriptionsByDay = GetSubscriptionsByDay(subscriptions, now);
        viewModel.RevenueByDay = GetRevenueByDay(subscriptions, now);
        viewModel.SubscriptionsByType = GetSubscriptionsByType(subscriptions);
        viewModel.ArticlesByStatus = GetArticlesByStatus(articles);
        viewModel.UsersByRole = GetUsersByRole(users, userRoles, roles);

        // Recent Activities
        viewModel.RecentArticles = articles
            .OrderByDescending(a => a.DateStamp)
            .Take(10)
            .Select(a => new RecentArticle
            {
                Id = a.Id,
                Headline = a.Headline,
                AuthorName = a.Author?.FullName ?? "Unknown",
                Status = a.ArticleStatus.ToString(),
                DateCreated = a.DateStamp,
                CategoryName = a.Category?.Name ?? "Unknown"
            })
            .ToList();

        viewModel.RecentSubscriptions = subscriptions
            .OrderByDescending(s => s.Created)
            .Take(10)
            .Select(s => new RecentSubscription
            {
                Id = s.Id,
                UserName = s.User?.FullName ?? "Unknown",
                UserEmail = s.User?.Email ?? "Unknown",
                SubscriptionType = s.SubscriptionType?.TypeName ?? "Unknown",
                Amount = s.SubscriptionType?.Price ?? 0,
                CreatedDate = s.Created,
                PaymentComplete = s.PaymentComplete
            })
            .ToList();

        // Top Performing Content
        viewModel.MostViewedArticles = articles
            .Where(a => a.ArticleStatus == ArticleStatus.Published)
            .OrderByDescending(a => a.Views)
            .Take(5)
            .Select(a => new TopArticle
            {
                Id = a.Id,
                Headline = a.Headline,
                AuthorName = a.Author?.FullName ?? "Unknown",
                Views = a.Views,
                Likes = a.Likes,
                CategoryName = a.Category?.Name ?? "Unknown",
                DatePublished = a.DateStamp
            })
            .ToList();

        viewModel.MostLikedArticles = articles
            .Where(a => a.ArticleStatus == ArticleStatus.Published)
            .OrderByDescending(a => a.Likes)
            .Take(5)
            .Select(a => new TopArticle
            {
                Id = a.Id,
                Headline = a.Headline,
                AuthorName = a.Author?.FullName ?? "Unknown",
                Views = a.Views,
                Likes = a.Likes,
                CategoryName = a.Category?.Name ?? "Unknown",
                DatePublished = a.DateStamp
            })
            .ToList();

        return viewModel;
    }

    private int GetUserCountByRole(List<User> users, List<IdentityUserRole<string>> userRoles, List<IdentityRole> roles, string roleName)
    {
        var roleId = roles.FirstOrDefault(r => r.Name == roleName)?.Id;
        if (roleId == null) return 0;

        var userIdsInRole = userRoles.Where(ur => ur.RoleId == roleId).Select(ur => ur.UserId).ToList();
        return users.Count(u => userIdsInRole.Contains(u.Id));
    }

    private List<ChartDataPoint> GetSubscriptionsByDay(List<Subscription> subscriptions, DateTime now)
    {
        var result = new List<ChartDataPoint>();
        var colors = new[] { "#007bff", "#28a745", "#ffc107", "#dc3545", "#6c757d", "#17a2b8", "#6f42c1" };

        for (int i = 6; i >= 0; i--)
        {
            var date = now.AddDays(-i).Date;
            var count = subscriptions.Count(s => s.Created.Date == date);
            result.Add(new ChartDataPoint
            {
                Label = date.ToString("MMM dd"),
                Value = count,
                Color = colors[6 - i]
            });
        }
        return result;
    }

    private List<ChartDataPoint> GetRevenueByDay(List<Subscription> subscriptions, DateTime now)
    {
        var result = new List<ChartDataPoint>();
        var colors = new[] { "#007bff", "#28a745", "#ffc107", "#dc3545", "#6c757d", "#17a2b8", "#6f42c1" };

        for (int i = 6; i >= 0; i--)
        {
            var date = now.AddDays(-i).Date;
            var revenue = subscriptions
                .Where(s => s.Created.Date == date)
                .Sum(s => s.SubscriptionType.Price);
            result.Add(new ChartDataPoint
            {
                Label = date.ToString("MMM dd"),
                Value = revenue,
                Color = colors[6 - i]
            });
        }
        return result;
    }

    private List<ChartDataPoint> GetSubscriptionsByType(List<Subscription> subscriptions)
    {
        var colors = new[] { "#007bff", "#28a745", "#ffc107", "#dc3545" };
        var groupedData = subscriptions
            .GroupBy(s => s.SubscriptionType.TypeName)
            .Select((g, index) => new ChartDataPoint
            {
                Label = g.Key,
                Value = g.Count(),
                Color = colors[index % colors.Length]
            })
            .ToList();
        return groupedData;
    }

    private List<ChartDataPoint> GetArticlesByStatus(List<Article> articles)
    {
        var colors = new[] { "#28a745", "#ffc107", "#dc3545", "#007bff", "#6c757d" };
        var statusCounts = new[]
        {
                new { Status = "Published", Count = articles.Count(a => a.ArticleStatus == ArticleStatus.Published) },
                new { Status = "Draft", Count = articles.Count(a => a.ArticleStatus == ArticleStatus.Draft) },
                new { Status = "Pending Review", Count = articles.Count(a => a.ArticleStatus == ArticleStatus.PendingReview) },
                new { Status = "Rejected", Count = articles.Count(a => a.ArticleStatus == ArticleStatus.Rejected) },
                new { Status = "Archived", Count = articles.Count(a => a.ArticleStatus == ArticleStatus.Archived) }
            };

        return statusCounts
            .Select((s, index) => new ChartDataPoint
            {
                Label = s.Status,
                Value = s.Count,
                Color = colors[index]
            })
            .ToList();
    }

    private List<ChartDataPoint> GetUsersByRole(List<User> users, List<Microsoft.AspNetCore.Identity.IdentityUserRole<string>> userRoles, List<Microsoft.AspNetCore.Identity.IdentityRole> roles)
    {
        var colors = new[] { "#007bff", "#28a745", "#ffc107", "#dc3545" };
        var roleCounts = new[]
        {
                new { Role = "Customers", Count = GetUserCountByRole(users, userRoles, roles, StaticConsts.Role_Customer) },
                new { Role = "Journalists", Count = GetUserCountByRole(users, userRoles, roles, StaticConsts.Role_Journalist) },
                new { Role = "Editors", Count = GetUserCountByRole(users, userRoles, roles, StaticConsts.Role_Editor) },
                new { Role = "Admins", Count = GetUserCountByRole(users, userRoles, roles, StaticConsts.Role_Admin) }
            };

        return roleCounts
            .Select((r, index) => new ChartDataPoint
            {
                Label = r.Role,
                Value = r.Count,
                Color = colors[index]
            })
            .ToList();
    }

    //[HttpPost]
    //public IActionResult DismissAdminView()
    //{
    //    StaticConsts.AdminView = false; 
    //    return RedirectToAction("SwitchToCustomerView");
    //}
    #endregion


}



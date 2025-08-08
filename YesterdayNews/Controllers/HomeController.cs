using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using YesterdayNews.Models;
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;

namespace YesterdayNews.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IArticleServices _articleServices;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ILogger<HomeController> logger, IArticleServices articleServices, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _articleServices = articleServices;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var latest = _articleServices.GetAllAsArticleVM(0, 3);
        return View(latest);
    }
    public IActionResult Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        var article = _articleServices.GetById(id, userId);
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

        var result = _articleServices.ToggleLike(userId, id);
        TempData["success"] = result ? "Article liked!" : "Article unliked!";

        return RedirectToAction("Details",  new { id });


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
}

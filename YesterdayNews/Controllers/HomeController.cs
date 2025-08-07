using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using YesterdayNews.Models;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;

namespace YesterdayNews.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IArticleServices _articleServices;
    public HomeController(ILogger<HomeController> logger, IArticleServices articleServices)
    {
        _logger = logger;
        _articleServices = articleServices;
    }

    public IActionResult Index()
    {
        var latest = _articleServices.GetAllAsArticleVM(0, 3);
        return View(latest);
    }
    public IActionResult Details(int id)
    {
        var article = _articleServices.GetOne(id);
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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

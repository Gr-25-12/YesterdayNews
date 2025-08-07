using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using YesterdayNews.Models;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;

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

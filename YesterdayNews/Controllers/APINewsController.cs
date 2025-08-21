using Microsoft.AspNetCore.Mvc;
using OutsourcedNews;

namespace YesterdayNews.Controllers
{
    public class APINewsController : Controller
    {
        private readonly NewsApiService _newsApiService;

        public APINewsController()
        {
            _newsApiService = new NewsApiService();
        }

        public async Task<IActionResult> TopNews()
        {
            var articles = await _newsApiService.GetTopNewsAsync("us");
            return View(articles);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _newsApiService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Components
{
    public class SidebarViewComponent : ViewComponent
    {
        // Added optional showMarkets parameter (default = true)
        public IViewComponentResult Invoke(int categoryId, bool showMarkets = true)
        {
            ViewData["SelectedCategory"] = categoryId;
            ViewData["ShowMarkets"] = showMarkets; // pass flag to the view
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Components
{
    public class SidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int categoryId)
        {
            ViewData["SelectedCategory"] = categoryId;
            return View();
        }
    }
}

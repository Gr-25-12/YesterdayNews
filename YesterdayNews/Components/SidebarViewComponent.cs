using Microsoft.AspNetCore.Mvc;

namespace YesterdayNews.Components
{
    public class SidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

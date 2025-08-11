using Microsoft.AspNetCore.Mvc.Rendering;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Models.ViewModels
{
    public class RoleMangementVM
    {
        public User ApplicationUser { get; set; }
        public IEnumerable<SelectListItem> RoleList { get; set; }
    }
}

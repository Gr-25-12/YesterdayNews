using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using System.Security.Claims;
using YesterdayNews.Data;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;


namespace YesterdayNews.Controllers
{



    public class UserController : Controller
    {

        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {

            _userService = userService;
        }

        [Authorize(Roles = StaticConsts.Role_Admin)]
        public IActionResult Index() { return View(); }
        [Authorize(Roles = StaticConsts.Role_Admin)]
        public IActionResult RoleMangement(string userId)
        {
            var vm = _userService.GetRoleManagementVm(userId);
            return View(vm);
        }

        [Authorize(Roles = StaticConsts.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> RoleMangement(RoleMangementVM roleVM)
        {
            await _userService.UpdateUserRoleAsync(roleVM);
            TempData["success"] = "User updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = StaticConsts.Role_Admin)]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var loggedInUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var users = _userService.GetAllUsers(loggedInUserId);

            return Json(new { data = users });
        }

        [Authorize(Roles = StaticConsts.Role_Admin + "," + StaticConsts.Role_Customer)]
        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody] string id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var loggedInUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var result = await _userService.LockUnlockUserAsync(id, loggedInUserId, User.IsInRole(StaticConsts.Role_Admin));

            return Json(new { success = result.Success, message = result.Message });
        }
    }
}

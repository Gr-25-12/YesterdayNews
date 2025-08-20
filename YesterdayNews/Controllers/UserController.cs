using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using System.Security.Claims;
using YesterdayNews.Data;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Utils;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace YesterdayNews.Controllers
{
  
        
       
        public class UserController : Controller
        {
            private readonly ApplicationDbContext _db;
            private readonly UserManager<IdentityUser> _userManager;

            public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
            {
                _db = db;
                _userManager = userManager;
            }
        [Authorize(Roles = StaticConsts.Role_Admin)]
        public IActionResult Index()
            {
                return View();
            }
        [Authorize(Roles = StaticConsts.Role_Admin)]
        public IActionResult RoleMangement(string userId)
            {
                string RoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

                RoleMangementVM roleVM = new()
                {
                    ApplicationUser = _db.Users
                    .FirstOrDefault(u => u.Id == userId)
                    ,
                    RoleList = _db.Roles.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Name
                    })
                    
                };

                roleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;

                return View(roleVM);
            }
        [Authorize(Roles = StaticConsts.Role_Admin)]
        [HttpPost]
            public IActionResult RoleMangement(RoleMangementVM roleVM)
            {
                string RoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleVM.ApplicationUser.Id).RoleId;
                string oldeRole = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;

                if (roleVM.ApplicationUser.Role != oldeRole)
                {
                    // a role is differnet
                    var applicationUser = _db.Users.FirstOrDefault(u => u.Id == roleVM.ApplicationUser.Id);
                   
                    _db.SaveChanges();
                    _userManager.RemoveFromRoleAsync(applicationUser, oldeRole).GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(applicationUser, roleVM.ApplicationUser.Role).GetAwaiter().GetResult();

                }

                TempData["success"] = "User updated succssefully!";
                return RedirectToAction(nameof(Index));
            }


        #region API CALLS
        [Authorize(Roles = StaticConsts.Role_Admin)]
        public IActionResult GetAll()
            {

            var clamisIdentity = (ClaimsIdentity) User.Identity;
            var loggedInUserId = clamisIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


                var usersList = _db.Users.Where(u=>u.Id!= loggedInUserId)
                .ToList();
                var userRoles = _db.UserRoles.ToList();
                var roles = _db.Roles.ToList();
                foreach (var user in usersList)
                {

                    var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                    user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                   
                }
                return Json(new { data = usersList });
            }

        [Authorize(Roles = StaticConsts.Role_Admin + "," + StaticConsts.Role_Customer)]
        [HttpPost]
            public IActionResult LockUnlock([FromBody] string id)
            {

            var userFromDb = _db.Users.FirstOrDefault(u => u.Id == id);
            var clamisIdentity = (ClaimsIdentity)User.Identity;
            var loggedInUserId = clamisIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (User.IsInRole(StaticConsts.Role_Admin) && id == loggedInUserId)
            {
                return Json(new
                {
                    success = false,
                    message = "Admins cannot delete their own accounts.",
                    isAdminSelfDelete = true
                });
            }

            if (userFromDb == null)
                {
                    return Json(new { success = false, message = "Error while Lokcing" });

                }

                if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.UtcNow)
                {
                    // user is currently locked
                    userFromDb.LockoutEnd = DateTime.UtcNow;
                TempData["success"]= "User unlcoked " ;

                }
                else
                {
                    userFromDb.LockoutEnd = DateTime.UtcNow.AddYears(100);
                TempData["success"] = "User deleted 🙋‍🙋‍ , bye bye! ";

            }
                _db.SaveChanges();
                return Json(new { success = true, message = TempData["success"] });
            }

            #endregion
        }
    }

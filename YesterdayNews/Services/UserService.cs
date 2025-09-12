using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public UserService(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public List<User> GetAllUsers()
        {
            return _userManager.Users.OfType<User>().ToList();
        }

        public List<IdentityUserRole<string>> GetUserRoles()
        {
            return _db.UserRoles.ToList();
        }

        public List<IdentityRole> GetRoles()
        {
            return _db.Roles.ToList();
        }
        public RoleMangementVM GetRoleManagementVm(string userId)
        {
            string roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

            var vm = new RoleMangementVM
            {
                ApplicationUser = _db.Users.FirstOrDefault(u => u.Id == userId),
                RoleList = _db.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                })
            };

            vm.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;

            return vm;
        }

        public async Task UpdateUserRoleAsync(RoleMangementVM roleVM)
        {
            string roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleVM.ApplicationUser.Id).RoleId;
            string oldRole = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;

            if (roleVM.ApplicationUser.Role != oldRole)
            {
                var applicationUser = _db.Users.FirstOrDefault(u => u.Id == roleVM.ApplicationUser.Id);

                await _userManager.RemoveFromRoleAsync(applicationUser, oldRole);
                await _userManager.AddToRoleAsync(applicationUser, roleVM.ApplicationUser.Role);

                _db.SaveChanges();
            }
        }

        public IEnumerable<User> GetAllUsers(string loggedInUserId)
        {
            var users = _db.Users.Where(u => u.Id != loggedInUserId).ToList();
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in users)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            }

            return users;
        }

        public async Task<(bool Success, string Message)> LockUnlockUserAsync(string id, string loggedInUserId, bool isAdmin)
        {
            var userFromDb = _db.Users.FirstOrDefault(u => u.Id == id);

            if (isAdmin && id == loggedInUserId)
            {
                return (false, "Admins cannot delete their own accounts.");
            }

            if (userFromDb == null)
                return (false, "Error while Locking");

            if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.UtcNow)
            {
                userFromDb.LockoutEnd = DateTime.UtcNow;
                _db.SaveChanges();
                return (true, "User unlocked");
            }
            else
            {
                userFromDb.LockoutEnd = DateTime.UtcNow.AddYears(100);
                _db.SaveChanges();
                return (true, "User deleted 🙋‍🙋‍ , bye bye!");
            }
        }
    }
}

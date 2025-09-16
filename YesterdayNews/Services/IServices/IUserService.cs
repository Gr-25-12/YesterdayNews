using Microsoft.AspNetCore.Identity;
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{
    public interface IUserService
    {
        List<User> GetAllUsers();
        List<IdentityUserRole<string>> GetUserRoles();
        List<IdentityRole> GetRoles();

        RoleMangementVM GetRoleManagementVm(string userId);
        Task UpdateUserRoleAsync(RoleMangementVM roleVM);
        IEnumerable<User> GetAllUsers(string loggedInUserId);
        Task<(bool Success, string Message)> LockUnlockUserAsync(string id, string loggedInUserId, bool isAdmin);
    }
}

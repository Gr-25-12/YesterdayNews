using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.Db;
using YesterdayNews.Utils;

[Authorize(Roles = StaticConsts.Role_Customer)]
public class LikeController : Controller
{
    private readonly ILikeService _likeService;
    private readonly UserManager<IdentityUser> _userManager;

    public LikeController(ILikeService likeService, UserManager<IdentityUser> userManager)
    {
        _likeService = likeService;
        _userManager = userManager;
    }

    [HttpPost]
    public  IActionResult ToggleLike(int articleId)
    {
        var userId = _userManager.GetUserId(User);
        var result =  _likeService.ToggleLike(userId, articleId);
        var likeCount =  _likeService.GetLikeCount(articleId);

        return Json(new
        {
            success = true,
            isLiked = result,
            likeCount
        });
    }
}
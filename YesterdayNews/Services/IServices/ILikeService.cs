using YesterdayNews.Models.Db;

public interface ILikeService
{
    UserArticleLike GetUserLikeFromDB(string userId, int articleId);
    bool ToggleLike(string userId, int articleId);
    //int GetLikeCount(int articleId);
}
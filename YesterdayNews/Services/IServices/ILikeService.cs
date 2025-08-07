public interface ILikeService
{
    bool ToggleLike(string userId, int articleId);
    int GetLikeCount(int articleId);
    bool IsArticleLikedByUser(string userId, int articleId);
}
using Microsoft.EntityFrameworkCore;
using YesterdayNews.Data;


namespace YesterdayNews.Services
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _db;

        public LikeService(ApplicationDbContext db)
        {
            _db = db;
        }

        public UserArticleLike GetUserLikeFromDB(string userId, int articleId)
        {
            var like = _db.UserArticleLikes
                .Include(l => l.Article)
                .Include(l => l.User)
                .FirstOrDefault(x => x.UserId == userId && x.ArticleId == articleId);
            return like;
        }

        public bool ToggleLike(string userId, int articleId)
        {
            var like = GetUserLikeFromDB(userId, articleId);

            if (like != null)
            {
                _db.UserArticleLikes.Remove(like);
                like.Article.Likes--;
                _db.SaveChanges();
                return false;
            }
            else
            {
                var newLike = new UserArticleLike
                {
                    UserId = userId,
                    ArticleId = articleId,
                    LikedAt = DateTime.UtcNow
                };
                _db.UserArticleLikes.Add(newLike);
                var article = _db.Articles.FirstOrDefault(m => m.Id == articleId);
                article.Likes++;
                _db.SaveChanges();
                return true;
            }

        }

        //public int GetLikeCount(int articleId)
        //{
        //        return _db.UserArticleLikes
        //            .Count(x => x.ArticleId == articleId);
        //}
    }
}
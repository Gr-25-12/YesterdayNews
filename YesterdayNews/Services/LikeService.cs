using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace YesterdayNews.Services
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _db;

        public LikeService(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool ToggleLike(string userId, int articleId)
        {
            
                var existingLike = _db.UserArticleLikes
                    .FirstOrDefault(x => x.UserId == userId && x.ArticleId == articleId);

                if (existingLike != null)
                {
                    // Unlike - remove the like
                    _db.UserArticleLikes.Remove(existingLike);
                    _db.SaveChanges();
                    return false; 
                }
                else
                {
                    // Like - add new like
                    var newLike = new UserArticleLike
                    {
                        UserId = userId,
                        ArticleId = articleId,
                        LikedAt = DateTime.UtcNow
                    };
                    _db.UserArticleLikes.Add(newLike);
                    _db.SaveChanges();
                    return true; 
                }
           
        }

        public int GetLikeCount(int articleId)
        {
            
                return _db.UserArticleLikes
                    .Count(x => x.ArticleId == articleId);
            
           
        }

        public bool IsArticleLikedByUser(string userId, int articleId)
        {
            
                if (string.IsNullOrEmpty(userId))
                    return false;

                return _db.UserArticleLikes
                    .Any(x => x.UserId == userId && x.ArticleId == articleId);
          
        }

    }
}
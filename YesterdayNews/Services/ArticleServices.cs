using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class ArticleServices : IArticleServices
    {
        private readonly ApplicationDbContext _db;
        private readonly ILikeService _likeService;


        public ArticleServices(ApplicationDbContext db, ILikeService likeService)
        {
            _db = db;
            _likeService = likeService;

        }

        public List<Article> GetAll()
        {
            
            return _db.Articles.Include(a => a.Author)
                               .Include(a => a.Category)
                               .OrderByDescending(a => a.DateStamp)
                               .ToList();
                
        }
        public List<ArticleVM> GetAllAsArticleVM(int articlesToSkip, int numberOfArticles)
        {
            return GetAll()
                   .Where(a => a.ArticleStatus == ArticleStatus.Published)
                   .Skip(articlesToSkip)
                   .Take(numberOfArticles)
                   .Select(a => new ArticleVM
                   {
                       Id = a.Id,
                       Headline = a.Headline,
                       Summary = a.ContentSummary,
                       ImageURL = a.ImageLink,
                       Linktext = a.LinkText,
                       Category = a.Category,
                       DateStamp = a.DateStamp
                   })
                   .ToList();

        }
        public List<ArticleVM> GetAllAsArticleVM(string query)
        {
            return _db.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Where(a => a.Headline.Contains(query) ||
                            a.ContentSummary.Contains(query) ||
                            a.Content.Contains(query)
                )
                .Select(a => new ArticleVM
                {
                    Id = a.Id,
                    Headline = a.Headline,
                    Summary = a.ContentSummary,
                    ImageURL = a.ImageLink,
                    Linktext = a.LinkText,
                    Category = a.Category,
                    DateStamp = a.DateStamp
                })
                .OrderByDescending(a => a.DateStamp)
                .ToList();
        }
        public List<ArticleVM> GetMostViewedArticleVM(int numberOfArticles)
        {
            return GetAll()
                   .Where(a => a.ArticleStatus == ArticleStatus.Published)
                   .OrderByDescending(a => a.Views)
                   .Take(numberOfArticles)
                   .Select(a => new ArticleVM
                   {
                       Id = a.Id,
                       Headline = a.Headline,
                       Summary = a.ContentSummary,
                       ImageURL = a.ImageLink,
                       Linktext = a.LinkText,
                       Category = a.Category,
                       DateStamp = a.DateStamp
                   })
                   .ToList();
        }
        public List<ArticleVM> GetMostLikedArticleVM(int numberOfArticles)
        {
            return GetAll()
                   .Where(a => a.ArticleStatus == ArticleStatus.Published)
                   .OrderByDescending(a => a.Likes)
                   .Take(numberOfArticles)
                   .Select(a => new ArticleVM
                   {
                       Id = a.Id,
                       Headline = a.Headline,
                       Summary = a.ContentSummary,
                       ImageURL = a.ImageLink,
                       Linktext = a.LinkText,
                       Category = a.Category,
                       DateStamp = a.DateStamp
                   })
                   .ToList();
        }

        public Article GetOne(int id)
        {
            
            var article = _db.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .FirstOrDefault(m => m.Id == id);
            return article;
        }

        public void Delete(int id)
        {
            var article = _db.Articles.FirstOrDefault(m => m.Id == id);
            if (article == null)
                throw new Exception("Articale not found.");

            _db.Articles.Remove(article);
            _db.SaveChanges();
        }

        public void Add(Article article)
        {
            _db.Articles.Add(article);
            _db.SaveChanges(true);
        }

        public void Edit(Article article)
        {
            _db.Articles.Update(article);
            _db.SaveChanges();
        }

        //overloading
        public Article GetById(int id, string currentUserId = null)
        {
            var article = _db.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.LikedByUsers)
                .FirstOrDefault(m => m.Id == id);

             // Update the Likes count from the actual likes table
            if (article != null)
            {
                article.Likes = _likeService.GetLikeCount(id);

                // Check if current user liked this article
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    article.IsLikedByCurrentUser = _likeService.IsArticleLikedByUser(currentUserId, id);
                }
                else
                {
                    article.IsLikedByCurrentUser = false;
                }
            }

            return article;
        }

        public bool ToggleLike(string userId, int articleId)
        {
            var existingLike = _db.UserArticleLikes
                .FirstOrDefault(x => x.UserId == userId && x.ArticleId == articleId);

            if (existingLike != null)
            {
                // Unlike
                _db.UserArticleLikes.Remove(existingLike);
                _db.SaveChanges();
                return false;
            }
            else
            {
                // Like
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
            return _likeService.GetLikeCount(articleId);
        }

        
        public void IncrementViews(int articleId)
        {
            var article = _db.Articles.FirstOrDefault(a => a.Id == articleId);
            if (article != null)
            {
                article.Views++;
                _db.SaveChanges();
            }
        }

      
    }
}

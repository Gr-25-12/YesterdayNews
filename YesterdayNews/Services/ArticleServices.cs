using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace YesterdayNews.Services
{
    public class ArticleServices : IArticleServices
    {
        private readonly ApplicationDbContext _db;



        public ArticleServices(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Article> GetAll()
        {
            return _db.Articles.Include(a => a.Author)
                               .Include(a => a.Category)
                               .OrderByDescending(a => a.DateStamp); 
        }
        public async Task<List<ArticleVM>> GetAllPublishedByCategoryAsArticleVM(int articlesToSkip, int numberOfArticles, int categoryId)
        {
            List<ArticleVM> result = new List<ArticleVM>();
            if (categoryId == 0)
            {
                result = await GetAll()
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
                       .ToListAsync();
            }
            else {
                result = await GetAll()
                       .Where(a => a.ArticleStatus == ArticleStatus.Published)
                       .Where(a => a.CategoryId == categoryId)
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
                       .ToListAsync();
            }
                return result;
        }
        public async Task<List<ArticleVM>> GetAllAsArticleVM(string query = "", bool archived = false)
        {
            IQueryable<Article> articles = _db.Articles
                .Include(a => a.Author)
                .Include(a => a.Category);

            if (archived)
                articles = articles.Where(a => a.ArticleStatus == ArticleStatus.Archived);
            else
                articles = articles.Where(a => a.ArticleStatus == ArticleStatus.Published);

            if (!string.IsNullOrEmpty(query))
            {
                articles = articles.Where(a =>
                    a.Headline.Contains(query) ||
                    a.ContentSummary.Contains(query) ||
                    a.LinkText.Contains(query) ||
                    a.Content.Contains(query) ||
                    a.Category.Name.Contains(query) ||
                    a.Author.FirstName.Contains(query) ||
                    a.Author.LastName.Contains(query)
                );
            }
            var result = await articles.Select(a => new ArticleVM
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
                .ToListAsync();
            return result;
        }
        public async Task<List<ArticleVM>> GetMostViewedArticleVM(int numberOfArticles)
        {
            return await GetAll()
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
                   .ToListAsync();
        }
        public async Task<List<ArticleVM>> GetMostLikedArticleVM(int numberOfArticles)
        {
            return await GetAll()
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
                   .ToListAsync();
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

        public async Task<int> TryArchiveOldArticles(DateTime archiveThreshold)
        {
            var published = await _db.Articles
                .Where(a => a.ArticleStatus == ArticleStatus.Published)
                .OrderBy(a => a.DateStamp)
                .ToListAsync();
            int archived = 0;
            foreach (Article article in published)
            {
                if (article.DateStamp <= archiveThreshold)
                {
                    article.ArticleStatus = ArticleStatus.Archived;
                    archived++;
                }
                else
                {
                    break;
                }
            }
            await _db.SaveChangesAsync();

            return archived;
        }

        public Article GetById(int id)
        {
            var article = _db.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.LikedByUsers)
                .FirstOrDefault(m => m.Id == id); 

            return article;
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
        public bool IsArticleLikedByUser(Article article, string userId)
        {
            if (!string.IsNullOrEmpty(userId) && article != null)
            {
                foreach (var like in article.LikedByUsers)
                {
                    if (like.UserId == userId)
                        return true;
                }
            }
            return false;
        }
    }
}

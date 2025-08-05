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


        public ArticleServices(ApplicationDbContext db)
        {
            _db = db;
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


    }
}

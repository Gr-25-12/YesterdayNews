using Microsoft.EntityFrameworkCore;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
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

        public Article GetOne(int id)
        {
            
            var article = _db.Articles.FirstOrDefault(m => m.Id == id);
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

        public void Edit(Article article)
        {
            _db.Articles.Update(article);
            _db.SaveChanges();
        }


    }
}

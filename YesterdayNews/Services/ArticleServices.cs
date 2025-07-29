using Microsoft.AspNetCore.Identity;
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

        public void Add(Article article)
        {
            _db.Articles.Add(article);
            _db.SaveChanges(true);
        }
        public Article GetById(int id)
        {
            var article = _db.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .FirstOrDefault(m => m.Id == id);

            return article;
        }
        //Temporary method? move to CategoryServices or wit for that to be done then delete this?
        public List<Category> GetAllCategories()
        {
            return _db.Categories.ToList();
        }
        //Temporary method? move to CategoryServices or wit for that to be done then delete this?
        public Category GetCategory(int id)
        {
            var category = _db.Categories.FirstOrDefault(c => c.Id == id);
            return category;
        }

    }
}

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
            return _db.Articles.ToList();
        }
    }
}

using YesterdayNews.Models.Db;

namespace YesterdayNews.Services.IServices
{
    public interface IArticleServices
    {
        List<Article> GetAll();
        Article GetOne(int id);
        void Delete(int id);
        void Add(Article article);
        Article GetById(int id);
        void Edit(Article existing);
    }
}

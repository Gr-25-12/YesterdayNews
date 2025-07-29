using YesterdayNews.Models.Db;

namespace YesterdayNews.Services.IServices
{
    public interface IArticleServices
    {
        List<Article> GetAll();
        Article GetOne(int id);
        void Delete(int id);
        //void Update(Article existing);
        void Edit(Article existing);
    }
}

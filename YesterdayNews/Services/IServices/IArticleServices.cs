using YesterdayNews.Models.Db;

namespace YesterdayNews.Services.IServices
{
    public interface IArticleServices
    {
        List<Article> GetAll();
    }
}

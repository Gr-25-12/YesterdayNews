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

        Article GetById(int id, string currentUserId = null);
        bool ToggleLike(string userId, int articleId);
        int GetLikeCount(int articleId);
        void IncrementViews(int articleId);
    }
}

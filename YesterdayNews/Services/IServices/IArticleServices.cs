using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{
    public interface IArticleServices
    {
        IQueryable<Article> GetAll();
        Task<List<ArticleVM>> GetAllPublishedByCategoryAsArticleVM(int articlesToSkip, int numberOfArticles, int categoryId);
        Task<List<ArticleVM>> GetAllAsArticleVM(string query = "", bool archived = false);
        Task<List<ArticleVM>> GetMostViewedArticleVM(int numberOfArticles);
        Task<List<ArticleVM>> GetMostLikedArticleVM(int numberOfArticles);
        void Delete(int id);
        void Add(Article article);
        void Edit(Article existing);
        Task<int> TryArchiveOldArticles(DateTime archiveThreshold);
        Article GetById(int id);
        void IncrementViews(int articleId);
        bool IsArticleLikedByUser(Article article, string userId);
    }
}

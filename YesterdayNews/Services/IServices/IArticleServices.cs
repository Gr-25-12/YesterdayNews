using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{
    public interface IArticleServices
    {
        List<Article> GetAll();
        List<ArticleVM> GetAllAsArticleVM(int articlesToSkip, int numberOfArticles);
        List<ArticleVM> GetMostViewedArticleVM(int numberOfArticles);
        List<ArticleVM> GetMostLikedArticleVM(int numberOfArticles);
        Article GetOne(int id);
        void Delete(int id);
        void Add(Article article);
        void Edit(Article existing);
    }
}

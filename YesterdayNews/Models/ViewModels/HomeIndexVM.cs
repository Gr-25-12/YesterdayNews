using System.Collections.Generic;
using YesterdayNews.Models;         // for NewsArticle
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels; // for ArticleVM

namespace YesterdayNews.Models.ViewModels
{
    public class HomeIndexVM
    {
        // Latest articles from your database
        public List<ArticleVM> LatestArticles { get; set; } = new();

        // News from NewsAPI
        public List<NewsArticle> ApiNews { get; set; } = new();
    }
}

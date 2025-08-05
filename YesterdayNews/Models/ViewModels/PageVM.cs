namespace YesterdayNews.Models.ViewModels
{
    public class PageVM
    {
        public List<ArticleVM> LatestNews { get; set; }
        public List<ArticleVM> Next10News { get; set; }
    }
}

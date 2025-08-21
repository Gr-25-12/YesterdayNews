using System;
using System.Collections.Generic;

namespace YesterdayNews.Models
{
    // Root response from NewsAPI
    public class NewsApiResponse
    {
        public string Status { get; set; }
        public int TotalResults { get; set; }
        public List<NewsArticleJson> Articles { get; set; }
    }

    // Each article from NewsAPI
    public class NewsArticleJson
    {
        public NewsSource Source { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string UrlToImage { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string Content { get; set; }
    }

    // The "source" object inside each article
    public class NewsSource
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutsourcedNews.Models
{
    public class NewsApiArticle
    {
        public NewsApiSource Source { get; set; } = new NewsApiSource();
        public string? Author { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? UrlToImage { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? Content { get; set; }
    }
}

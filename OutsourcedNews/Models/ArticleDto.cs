using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutsourcedNews.Models
{
   public class ArticleDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string UrlToImage { get; internal set; }
    }
}

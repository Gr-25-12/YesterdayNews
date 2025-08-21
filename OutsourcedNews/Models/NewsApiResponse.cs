using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutsourcedNews.Models
{
    public class NewsApiResponse
    {
        public string Status { get; set; } = string.Empty;
        public int TotalResults { get; set; }
        public List<NewsApiArticle> Articles { get; set; } = new List<NewsApiArticle>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutsourcedNews
{
    class NewsApiConfiguration
    {
        public const string SectionName = "NewsApi";

        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://newsapi.org/v2";
        public int DefaultPageSize { get; set; } = 10;
        public int TimeoutSeconds { get; set; } = 30;
    }
}

using System.Net.Http;
using System.Text.Json;
using YesterdayNews.Models.Db;

namespace YesterdayNews.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "bd50a8370a3f420083b42dc69292f5d5";

        public NewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
           
        public async Task<List<NewsArticle>> GetTopHeadlinesAsync(string country = "us", int pageSize = 10)
        {
            try
            {
                //var url = $"https://newsapi.org/v2/top-headlines?country={country}&pageSize={pageSize}&apiKey={ApiKey}";
                var url = $"https://newsapi.org/v2/top-headlines?country=us&apiKey=bd50a8370a3f420083b42dc69292f5d5";
                var response = await _httpClient.GetAsync("https://newsapi.org/v2/top-headlines?country=us&apiKey=bd50a8370a3f420083b42dc69292f5d5");

                if (!response.IsSuccessStatusCode)
                {
                    // Log the status code for debugging
                    Console.WriteLine($"NewsAPI returned {response.StatusCode}");
                    return new List<NewsArticle>
                        
                        {
                             new NewsArticle { Title = "Sample News 1", Url = "#", SourceName = "NewsAPI", PublishedAt = DateTime.Now },
                             new NewsArticle { Title = "Sample News 2", Url = "#", SourceName = "NewsAPI", PublishedAt = DateTime.Now }
                        };

                }

                var json = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(json);

                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (newsResponse?.Articles == null) return new List<NewsArticle>();

                return newsResponse.Articles.Select(a => new NewsArticle
                {
                    Title = a.Title,
                    Description = a.Description,
                    Url = a.Url,
                    UrlToImage = a.UrlToImage,
                    PublishedAt = a.PublishedAt,
                    SourceName = a.Source?.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NewsAPI fetch failed: {ex.Message}");
                return new List<NewsArticle>();
            }
        }


        // Internal classes to match JSON
        private class NewsApiResponse
        {
            public string Status { get; set; }
            public int TotalResults { get; set; }
            public List<NewsArticleJson> Articles { get; set; }
        }

        private class NewsArticleJson
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public string UrlToImage { get; set; }
            public DateTime? PublishedAt { get; set; }
            public NewsSource Source { get; set; } 

        }

        private class NewsSource
        {
            public string Name { get; set; }
        }
    }
}

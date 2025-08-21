using Microsoft.Extensions.Configuration;
using OutsourcedNews.Models;
using System.Text.Json;

namespace OutsourcedNews
{
    public class NewsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private string BASE_URL { get; set; }

        public NewsApiService(
            //IConfiguration config
            )
        {
            _httpClient = new HttpClient();
            //_config = config;
            //BASE_URL = _config["NewsAPI:URL"]!;
        }

        public async Task<List<ArticleDto>> GetTopNewsAsync(string country = "us")
        {
            try
            {
                //var url = BASE_URL;
                var url = "https://newsapi.org/v2/top-headlines?country=us&apiKey=bd50a8370a3f420083b42dc69292f5d5";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Error: {response.StatusCode}");
                    return new List<ArticleDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Articles == null)
                    return new List<ArticleDto>();

                return apiResponse.Articles.Select(article => new ArticleDto
                {
                    Title = article.Title ?? "No Title",
                    Description = article.Description ?? "",
                    Url = article.Url ?? "",
                    UrlToImage = article.UrlToImage ?? "",
                    PublishedAt = article.PublishedAt,
                    SourceName = article.Source?.Name ?? "",
                    Author = article.Author ?? ""
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching news: {ex.Message}");
                return new List<ArticleDto>();
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
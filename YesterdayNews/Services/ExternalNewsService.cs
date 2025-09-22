using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using YesterdayNews.Models.Db;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class ExternalNewsService : IExternalNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private string BASE_URL { get; set; }

        public ExternalNewsService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            BASE_URL = _config["NewsAPI:URL"]!;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YesterdayNewsApp/1.0");
        }



        public async Task<List<ExternalNewsVM>> GetTopNewsAsync([FromServices] IWebHostEnvironment env)
        {
            var url = string.Empty;
            var apiKeyProd = _config["NewsAPI:ApiKey"]!;
            var apiKeyDev = _config["NewsAPI:ApiKeyDev"]!;

            if (env.IsProduction())
            {
                url = $"https://api.thenewsapi.com/v1/news/top?api_token={apiKeyProd}&locale=us&limit=3";
            }
            else
            {
                url = $"https://newsapi.org/v2/top-headlines?country=us&pageSize=100&apiKey={apiKeyDev}";
            }

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return new List<ExternalNewsVM>();
            }

            var json = await response.Content.ReadAsStringAsync();

            // Toggling
            if (env.IsProduction())
            {

                var prodApiResponse = JsonSerializer.Deserialize<NewsApiResponseProd>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                return ConvertProdToStandard(prodApiResponse?.Articles!);
            }
            else
            {

                var devApiResponse = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return devApiResponse?.Articles ?? new List<ExternalNewsVM>();
            }
        }

        public async Task<List<ExternalNewsVMSEBY>> GetTopFromSEBY()
        {
            var response = await _httpClient.GetAsync("https://api.sebynews.v7.ua/api/articles?PageSize=3");
            var content = await response.Content.ReadAsStringAsync();

            var SebyArticles = JsonSerializer.Deserialize<SebyArticlesResponse>(content);

            return SebyArticles?.Articles;
        }
        private List<ExternalNewsVM> ConvertProdToStandard(List<ExternalNewsVMProd> prodArticles)
        {
            return prodArticles.Select(prodArticle => new ExternalNewsVM
            {
                Title = prodArticle.Title ?? string.Empty,
                Description = prodArticle.Description,
                Url = prodArticle.Url ?? string.Empty,
                UrlToImage = prodArticle.UrlToImage,
                PublishedAt = prodArticle.PublishedAt,
                Author = null,
                Content = prodArticle.Content,
                Source = new NewsSource
                {
                    Name = prodArticle.Source ?? "Unknown Source",
                    Id = null
                }
            }).ToList();
        }


    }
}


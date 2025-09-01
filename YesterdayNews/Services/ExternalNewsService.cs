using System.Text.Json;
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
            _config=config;
            BASE_URL = _config["NewsAPI:URL"]!;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YesterdayNewsApp/1.0");
        }


        public async Task<List<ExternalNewsVM>> GetTopNewsAsync()
        {

            var apiKey = _config["NewsAPI:ApiKey"]!;
            var url = $"https://newsapi.org/v2/top-headlines?country=us&pageSize=100&apiKey={apiKey}";
            var response = await _httpClient.GetAsync(url);
            //Only there to check if the API is reachable and throw meaningful error if not
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                
            }


            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });



            return apiResponse.Articles;

        }

    }
    }



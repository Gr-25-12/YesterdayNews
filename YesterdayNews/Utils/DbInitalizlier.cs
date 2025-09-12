using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;
using Quartz;

namespace YesterdayNews.Utils
{
    public class DbInitalizlier :IDbInitalizlier   , IJob
    {
        private readonly ApplicationDbContext _db; 
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private string apiKeyDev { get; set; }

        public DbInitalizlier(HttpClient httpClient, IConfiguration config , ApplicationDbContext db)
        {
            _httpClient = httpClient;
             _config = config;
            _db = db;
            apiKeyDev = _config["NewsAPI:ApiKeyDev"]!;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YesterdayNewsApp/1.0");
        }

        public Task Execute(IJobExecutionContext context)
        {
           return Initialize();
        }
        public async Task Initialize()
        {
            SeedData().GetAwaiter().GetResult();
        }

        public async Task SeedData()
        {
            var rand = new Random();
            var categories = _db.Categories.ToList();
            var categoryRandomIndex = rand.Next(categories.Count-1);

            var authors = new[] { "998b77da-f88e-410b-9f7c-d46673d2a4af", "6e6fa4cf-29c2-4cc2-9ba2-7f64534f52c5" };

            var url = $"https://newsapi.org/v2/top-headlines?country=us&pageSize=10&apiKey={apiKeyDev}";
            List<Article> articlesToSeedDatabse = new List<Article>();
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                var extractedNews = JsonSerializer.Deserialize<NewsApiResponse>(content);

                foreach (var article in extractedNews.Articles)
                {
                    var selectedCategory = categories[categoryRandomIndex];
                    var authorzId = authors[rand.Next(authors.Length)];
                    var artclie = new Article
                    {
                        CategoryId = selectedCategory.Id,
                        ContentSummary = article.Description ?? "No Description Found",
                        Headline = article.Title.Split("-")[0],
                        DateStamp = (DateTime)article.PublishedAt,
                        AuthorId = authorzId,
                        LinkText = article.Title.Substring(0, 40) + "..." ?? article.Title.Substring(0, 15),
                        Content = $"{article.Content}\n{article.Content}\n\n {article.Author}\n {article.SourceName}",
                        Views = rand.Next(200),
                        Likes = rand.Next(111),
                        ImageLink = article.UrlToImage ?? "https://placehold.co/600x400?text=Image+Not+Found",
                        ArticleStatus = ArticleStatus.Published
                    };

                    articlesToSeedDatabse.Add(artclie);
                }

                await _db.AddRangeAsync(articlesToSeedDatabse);
                await _db.SaveChangesAsync(); 
            }
            catch (Exception e)
            {

                throw e;
            }
            
            
        }

        
    }
}

using System;
using YesterdayNews.Models.Api.Weather;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class WeatherApiService : IWeatherApiService
    { 
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;


        public WeatherApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = "" + config["OpenMap:ApiKey"];
        }
        public async Task<DailyForecast.Rootobject>GetForecastByCityAsync(string city)
        {
            if (string.IsNullOrEmpty(city))
                return null;

            try {
            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetFromJsonAsync<DailyForecast.Rootobject>(url);
            if (response?.list == null)
                return new DailyForecast.Rootobject
                {
                    city = new DailyForecast.City { name = "stockholm", country = "SE" },
                    list = Array.Empty<DailyForecast.List>()
                };
            return response;
            }

            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Http error :{ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexcpected error :{ex.Message}");
            }
            return new DailyForecast.Rootobject
            {
                city = new DailyForecast.City { name = "stockholm", country = "SE" },
                list = Array.Empty<DailyForecast.List>()
            };


        }














    }
}

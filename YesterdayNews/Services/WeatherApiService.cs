
using YesterdayNews.Models.Api;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class WeatherApiService : IWeatherApiService
    { 
        private readonly HttpClient _httpClient;


        public WeatherApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Weather> GetWeatherAsync()
        {
            var weatherData = await _httpClient.GetFromJsonAsync<Weather>("https://weatherapi.dreammaker-it.se/Forecast?city=stockholm&lang=english");
            return weatherData;
        }



    }
}

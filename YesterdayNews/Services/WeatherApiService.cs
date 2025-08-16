
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

        public async Task<List<Weather>> GetWeatherByCityAsync(string city)
        {
            if (string.IsNullOrEmpty(city))
                return null;

            var url = $"https://weatherapi.dreammaker-it.se/Forecast/24Hours?location={Uri.EscapeDataString(city)}&lang=english";
            var forecasts = await _httpClient.GetFromJsonAsync<List<Weather>>(url);

            if (forecasts == null)
                return null;

            return FilterForecasts(forecasts);
        }


        private List<Weather> FilterForecasts(List<Weather> forecasts)
        {
            if (forecasts == null || forecasts.Count == 0)
                return new List<Weather>();

            int[] targetHours = new int[] { 6, 9, 12, 15, 18, 21, 0, 3 };
            var today = DateTime.Now.Date;

            return forecasts
                .Where(f => f.Date.Date == today && targetHours.Contains(f.Date.Hour))
                .OrderBy(f => f.Date.Hour)
                .ToList();
        }


        public Weather GetCurrentForecast(List<Weather> forecasts)
        {

            if (forecasts == null || !forecasts.Any())
                return null;

            var now = DateTime.Now;
            var closestForecast = forecasts
                .OrderBy(f => Math.Abs((f.Date - now).TotalMinutes))
                .FirstOrDefault();
            return closestForecast;
        }


    }
}

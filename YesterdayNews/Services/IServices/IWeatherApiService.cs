using YesterdayNews.Models.Api;

namespace YesterdayNews.Services.IServices
{
    public interface IWeatherApiService
    {
        Task<List<Weather>> GetWeatherByCityAsync(string city);
        Weather GetCurrentForecast(List<Weather> forecasts);

    }
}

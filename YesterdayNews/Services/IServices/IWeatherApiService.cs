using YesterdayNews.Models.Api.Weather;

namespace YesterdayNews.Services.IServices
{
    public interface IWeatherApiService
    {   
        Task<DailyForecast.Rootobject>GetForecastByCityAsync(string city);
     

    }
}

using YesterdayNews.Models.Api.Weather;
using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{
    public interface IWeatherApiService
    {
        Task<List<ForecastVM>> GetForecastByCityAsync();
        Task<List<ForecastVM>> GetForecastByCityAsync(string city);
        Task<List<ForecastVM>> GetForecastByCoordinatesAsync(double lat, double lon);
        Task RefreshPreloadedCitiesAsync();


    }


}


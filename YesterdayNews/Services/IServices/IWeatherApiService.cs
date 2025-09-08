using YesterdayNews.Models.Api.Weather;
using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{
    public interface IWeatherApiService
    {
        // multi-day (5-day) forecast
        Task<List<ForecastVM>> GetMultiDayForecastByCityAsync(string city);
        Task<List<ForecastVM>> GetMultiDayForecastByCoordAsync(double lat, double lon);

       //single day forecast
        Task<ForecastVM?> GetSingleCurrentForecastByCoordAsync(double lat, double lon);
     

        //background preloads
        Task RefreshPreloadedCitiesAsync();


    }


}


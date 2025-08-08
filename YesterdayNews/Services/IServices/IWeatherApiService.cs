using YesterdayNews.Models.Api;

namespace YesterdayNews.Services.IServices
{
    public interface IWeatherApiService
    {
       Task<Weather> GetWeatherAsync();
    }
}

﻿
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using YesterdayNews.Models.Api.Weather;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YesterdayNews.Models.Api.Weather.OpenWeatherMapModel;

namespace YesterdayNews.Services
{
    public class WeatherApiService : IWeatherApiService
    { 
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _apiKey;
        private readonly string _defaultCity;
    
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(3);
        private readonly List<string> _preloadedCities;


        public WeatherApiService(HttpClient httpClient, IConfiguration config ,IMemoryCache cache)
        {
           
            _httpClient = httpClient;
            _cache = cache;
            _apiKey = "" + config["OpenMap:ApiKey"];
            _defaultCity = config["OpenWeatherMap:DefaultCity"] ?? "Stockholm";
          
            _preloadedCities = WeatherPreloadedCities.Cities;
        }

        public async Task<List<ForecastVM>> GetForecastByCityAsync()
        {
            return await GetForecastByCityAsync(_defaultCity);
        }
        public async Task RefreshPreloadedCitiesAsync()
        {
            foreach (var city in _preloadedCities)
            {
                try
                {
                    await GetForecastByCityAsync(city);
                }
                catch (Exception ex)
                {
                    // Continue with other cities if one fails
                }
            }
        }

        public async Task<List<ForecastVM>> GetForecastByCityAsync(string city)
        {
            var cacheKey = $"weather_{city}";
            if (_cache.TryGetValue(cacheKey, out List<ForecastVM> cached))
                return cached;

            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<OpenWeatherMapModel.Rootobject>(response);
            var cachedForecast = ProjectForecastData(data!);

            _cache.Set(cacheKey, cachedForecast, _cacheDuration);
            return cachedForecast;
        }


        private static List<ForecastVM> ProjectForecastData(OpenWeatherMapModel.Rootobject response)
        {
            var cleanCityName = CleanCityName(response.city.name);
            
            return response.list
               .GroupBy(f => DateTime.Parse(f.dt_txt).Date)
               .OrderBy(g => g.Key)
               .Take(5)
               .SelectMany(day => day.OrderBy(f => DateTime.Parse(f.dt_txt))
                   .Select(f =>
                   {
                       var weather = f.weather.FirstOrDefault();
                       return new ForecastVM
                       {
                           City = cleanCityName,
                           Date = DateTime.Parse(f.dt_txt),
                           Summary = weather?.description ?? "No description",
                           TemperatureC = (int)Math.Round(f.main.temp),
                           IconUrl = weather != null ? $"http://openweathermap.org/img/wn/{weather.icon}@2x.png" : null
                       };
                   }))
               .ToList();
        }


        public async Task<List<ForecastVM>> GetForecastByCoordinatesAsync(double lat, double lon)
        {
            var cacheKey = $"weather_coords_{lat:F2}_{lon:F2}";
            if (_cache.TryGetValue(cacheKey, out List<ForecastVM> cached))
                return cached;

            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
                var response = await _httpClient.GetStringAsync(url);
                var data = JsonConvert.DeserializeObject<OpenWeatherMapModel.Rootobject>(response);

                if (data == null || data.list == null)
                    return new List<ForecastVM>();

                var result = ProjectCurrentForecastData(data);
                _cache.Set(cacheKey, result, _cacheDuration);
                return result;
            }
            catch (Exception ex)
            {
                
                return new List<ForecastVM>();
            }
        }




        private static List<ForecastVM> ProjectCurrentForecastData(OpenWeatherMapModel.Rootobject response)
        {
            var now = DateTime.Now;
            var cleanCityName = CleanCityName(response.city.name);
            
            var closestForecast = response.list  
                .Select(f => new
                {
                    ForecastTime = DateTime.Parse(f.dt_txt),
                    Item = f
                })
                .Where(x => x.ForecastTime >= now)

                .FirstOrDefault();

            if (closestForecast == null)
                return new List<ForecastVM>(); 

            var weather = closestForecast.Item.weather.FirstOrDefault();
            var currentWeather = new ForecastVM
            {
                City = cleanCityName,  
                Date = closestForecast.ForecastTime,
                Summary = weather?.description ?? "No description",
                TemperatureC = (int)Math.Round(closestForecast.Item.main.temp),
                IconUrl = weather != null ? $"http://openweathermap.org/img/wn/{weather.icon}@2x.png" : null
            };

            return new List<ForecastVM> { currentWeather };  
        }







        public async Task<List<ForecastVM>> GetCurrentWeatherByCityAsync(string city)
        {
            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<OpenWeatherMapModel.Rootobject>(response);

            return ProjectCurrentForecastData(data!);  
        }

    
        private static string CleanCityName(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return cityName;
                
            // Remove common municipality suffixes
            var suffixesToRemove = new[] { " Municipality", " Kommune", " Kommun" };
            
            foreach (var suffix in suffixesToRemove)
            {
                if (cityName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    return cityName.Substring(0, cityName.Length - suffix.Length).Trim();
                }
            }
            
            return cityName;
        }






    }



    }

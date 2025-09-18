
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Globalization;
using YesterdayNews.Models.Api.Weather;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;


namespace YesterdayNews.Services
{
    public class WeatherApiService : IWeatherApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _apiKey;
        private readonly ILogger<WeatherApiService> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(3);
        private readonly List<string> _preloadedCities;


        public WeatherApiService(HttpClient httpClient, ILogger<WeatherApiService> logger, IConfiguration config, IMemoryCache cache)
        {

            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
            _apiKey = "" + config["OpenMap:ApiKey"];
            _preloadedCities = WeatherPreloadedCities.Cities;
        }

        //API CALLS
        public async Task<List<ForecastVM>> GetMultiDayForecastByCityAsync(string city)
        {
            
            var normalizedCity = city?.Trim().ToLowerInvariant();
            var cacheKey = $"weather_{normalizedCity}";
            if (_cache.TryGetValue(cacheKey, out List<ForecastVM> cached))
                return cached;

            try
            {

                var url = $"data/2.5/forecast?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric";

                var response = await _httpClient.GetStringAsync(url);
                var data = JsonConvert.DeserializeObject<OpenWeatherMapModel.Rootobject>(response);
                var cachedForecast = ProjectMultiForecastData(data!);

                _cache.Set(cacheKey, cachedForecast, _cacheDuration);
                return cachedForecast;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Failed to fetch forecast for city: {City}", city);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetMultiDayForecastByCityAsync for city: {City}", city);
                return null;
            }
        }


        public async Task<List<ForecastVM>> GetMultiDayForecastByCoordAsync(double lat, double lon)
        {
            var cacheKey = $"weather_coords_multiday_{lat:F2}_{lon:F2}";
            if (_cache.TryGetValue(cacheKey, out List<ForecastVM> cached))
                return cached;

            var url = $"data/2.5/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";

            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<OpenWeatherMapModel.Rootobject>(response);
            var result = ProjectMultiForecastData(data!);

            _cache.Set(cacheKey, result, _cacheDuration);
            return result;
        }



        public async Task<ForecastVM?> GetSingleCurrentForecastByCoordAsync(double lat, double lon)
        {
            var cacheKey = $"weather_coords_{lat:F2}_{lon:F2}_single";
            if (_cache.TryGetValue(cacheKey, out ForecastVM? cached))
                return cached;

            try
            {
                var url = $"data/2.5/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";

                var response = await _httpClient.GetStringAsync(url);
                var data = JsonConvert.DeserializeObject<OpenWeatherMapModel.Rootobject>(response);

                if (data == null || data.list == null)
                    return null;

                var resultList = ProjectCurrentForecastData(data);
                var singleResult = resultList.FirstOrDefault();

                _cache.Set(cacheKey, singleResult, _cacheDuration);
                return singleResult;
            }
            catch (Exception ex)
            {
               
                return null;
            }
        }


        private async Task<OpenWeatherMapModel.Rootobject?> FetchForecastDataAsync(string city)
        {
            try
            {
                var url = $"data/2.5/forecast?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric";
                var response = await _httpClient.GetStringAsync(url);
                var data = JsonConvert.DeserializeObject<OpenWeatherMapModel.Rootobject>(response);
                return data;
            }
            catch
            {
                return null;
            }
        }

        //Filters ##################

        private static List<ForecastVM> ProjectMultiForecastData(OpenWeatherMapModel.Rootobject response)
        {
            if (response == null || response.list == null)
                return new List<ForecastVM>();

            var city = response.city?.name ?? "Unknown";
            var country = response.city?.country ?? "XX";
            var cleanCity = LocationUtils.CleanCityName(city);

            //  Parse all forecasts with original date
            var forecasts = response.list
                .Select(f => new
                {
                    Forecast = f,
                    Date = DateTime.ParseExact(f.dt_txt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
                })
                .ToList();

            // Adjust grouping to treat 00:00 as part of the previous day
            var groupedByAdjustedDate = forecasts
                .GroupBy(x =>
                    x.Date.TimeOfDay == TimeSpan.Zero
                        ? x.Date.Date.AddDays(-1)  // Treat 00:00 as previous day
                        : x.Date.Date
                )
                .OrderBy(g => g.Key)
                .Take(6) // Only take 6 grouped days
                .SelectMany(group => group.OrderBy(x => x.Date))
                .ToList();

            //  Project to ForecastVM
            return groupedByAdjustedDate
                .Select(x =>
                {
                    var weather = x.Forecast.weather?.FirstOrDefault();
                    return new ForecastVM
                    {
                        City = city,
                        Country = country,
                        DisplayLocation = $"{cleanCity}, {country}",
                        Date = x.Date, 
                        Summary = weather?.description ?? "No description",
                        TemperatureC = (int)Math.Round(x.Forecast.main.temp),
                        IconUrl = weather != null ? $"http://openweathermap.org/img/wn/{weather.icon}@2x.png" : null
                    };
                })
                .ToList();
        }


        private static List<ForecastVM> ProjectCurrentForecastData(OpenWeatherMapModel.Rootobject response)
        {
           
            if (response == null || response.list == null)
                return new List<ForecastVM>();


            var city = response.city?.name ?? "Unknown";
            var country = response.city?.country ?? "XX";
            var cleanCity = LocationUtils.CleanCityName(city);
            var now = DateTime.Now;


           
            var closestForecast = response.list
               
                .Select(f => new {
                    ForecastTime = DateTime.Parse(f.dt_txt),
                    Forecast = f
                })
             
                .Where(x => x.ForecastTime >= now)
             
                .OrderBy(x => x.ForecastTime)
                .FirstOrDefault();

            if (closestForecast == null)
                return new List<ForecastVM>();

            var weather = closestForecast.Forecast.weather?.FirstOrDefault();

      
            var currentWeather = new ForecastVM
            {
                City = city,
                Country = country,
                DisplayLocation = $"{cleanCity}, {country}",
                Date = closestForecast.ForecastTime,
                Summary = weather?.description ?? "No description",
                TemperatureC = (int)Math.Round(closestForecast.Forecast.main.temp),
                IconUrl = weather != null ? $"http://openweathermap.org/img/wn/{weather.icon}@2x.png" : null
            };

            return new List<ForecastVM> { currentWeather };
        }









        //Data Cache ##########
        public async Task RefreshPreloadedCitiesAsync()
        {
            foreach (var city in _preloadedCities)
            {
                try
                {
                 
                    var rawData = await FetchForecastDataAsync(city);

                    if (rawData == null)
                    {
                        Console.WriteLine($"No data returned for city '{city}'");
                        continue;
                    }

                   
                    var multiDay = ProjectMultiForecastData(rawData);
                    _cache.Set($"weather_{city.ToLowerInvariant()}", multiDay, _cacheDuration);

                   
                    var current = ProjectCurrentForecastData(rawData);
                    _cache.Set($"weather_current_{city.ToLowerInvariant()}", current, _cacheDuration);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to refresh forecast for '{city}': {ex.Message}");

                }
            }
        }

    




    }



}

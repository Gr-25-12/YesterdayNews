namespace YesterdayNews.Models.ViewModels
{
    public class ForecastVM
    {
            public string City { get; set; }
            public string Country { get; set; }
            public string DisplayLocation { get; set; }
        
            public DateTime Date { get; set; }
            public string Summary { get; set; }
            public int TemperatureC { get; set; }
            public int TemperatureF => (int)Math.Round(32 + (TemperatureC / 0.5556));
            public string IconUrl { get; set; }
        

    }

}

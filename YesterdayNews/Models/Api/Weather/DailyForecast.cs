namespace YesterdayNews.Models.Api.Weather
{
    public class DailyForecast
    {


        public class Rootobject
        {
            public City city { get; set; }
            public List[] list { get; set; }
        }

        public class City
        {
            public string name { get; set; }
            public string country { get; set; }
        }

        public class List
        {
            public string dt_txt { get; set; }
            public Main main { get; set; }
            public Weather[] weather { get; set; }
            public float pop { get; set; }
            
        }

        public class Main
        {
            public float temp{ get; set; }
            public float temp_min { get; set; }
            public float temp_max { get; set; }
        }



        public class Weather
        {
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }

    }
}

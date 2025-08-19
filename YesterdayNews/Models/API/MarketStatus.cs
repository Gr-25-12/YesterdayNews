namespace YesterdayNews.Models.API
{
    public class MarketStatus
    {
            public string exchange { get; set; }
            public object holiday { get; set; }
            public bool isOpen { get; set; }
            public string session { get; set; }
            public int t { get; set; }
            public string timezone { get; set; }

    }
}

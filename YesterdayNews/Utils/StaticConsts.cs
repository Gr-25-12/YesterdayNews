namespace YesterdayNews.Utils
{
    public static class StaticConsts
    {
        public const string ArticleDraft = "Draft";
        public const string ArticlePendingReview = "PendingReview";
        public const string ArticleRejected = "Rejected";
        public const string ArticlePublished = "Published";
        public const string ArticleArchived = "Archived";


        public const string Role_Customer = "Customer";
        public const string Role_Journalist = "Journalist";
        public const string Role_Admin = "Admin";
        public const string Role_Editor = "Editor";


        public const int Cookie_Expires_IN = 7;
        public const int ARTICLE_ARCHIVED_IN = 30; //Days


        public const string SubscriptionType_Monthly = "Monthly";
        public const string SubscriptionType_Yearly = "Yearly";
        public const string SubscriptionType_Quarterly = "Quarterly";
        public const string SubscriptionType_Weekly = "Weekly";


#if DEBUG
        public const string Home_URL = @"https://localhost:7195/";
#else
    public const string Home_URL = @"https://yesterdaynews.azurewebsites.net/";
#endif


        public const string YN_LOGO = "https://yesterdaystoragegr12.blob.core.windows.net/notarticles/YN_logo.png";


        public static bool AdminView = false;


        // Icons for markets 
        public static string GetIconClass(string symbol)
        {
            return symbol?.ToUpper() switch
            {
                "AAPL" => "fab fa-apple text-muted",
                "MSFT" => "fab fa-microsoft text-primary",
                "GOOGL" or "GOOG" => "fab fa-google text-danger",
                "NVDA" => "fab fa-nvidia text-green",
                "AMZN" => "fab fa-amazon text-warning",
                "TSLA" => "fab fa-tesla text-danger",
                "META" => "fab fa-meta text-info",
                "NFLX" => "fab fa-netflix text-danger",
                "JPM" => "fas fa-university text-info",
                "BAC" => "fas fa-piggy-bank text-warning",
                "XOM" => "fas fa-gas-pump text-danger",
                "WMT" => "fas fa-shopping-cart text-success",
                "PG" => "fas fa-soap text-primary",
                "DIS" => "fas fa-film text-warning",
                "V" or "MA" => "fas fa-credit-card text-success",
                _ => "fas fa-chart-bar text-muted"
            };
        }
        public static string GetCryptoClass(string symbol)
        {
            return symbol?.ToUpper() switch
            {
                "BINANCE:BTCUSDT" => "fab fa-bitcoin text-warning",   // Bitcoin
                "BINANCE:ETHUSDT" => "fab fa-ethereum text-dark",     // Ethereum
                "BINANCE:BNBUSDT" => "fab fa-binance text-warning",   // Binance Coin
                "BINANCE:SOLUSDT" => "fas fa-sun text-warning",       // Solana
                "BINANCE:XRPUSDT" => "fas fa-water text-primary",     // Ripple
                "BINANCE:ADAUSDT" => "fas fa-coins text-info",        // Cardano
                "BINANCE:AVAXUSDT" => "fas fa-mountain text-danger",  // Avalanche
                "BINANCE:DOGEUSDT" => "fas fa-dog text-warning",      // Dogecoin
                "BINANCE:DOTUSDT" => "fas fa-circle-notch text-danger", // Polkadot
                "BINANCE:MATICUSDT" => "fas fa-gem text-info",        // Polygon
                "BINANCE:LTCUSDT" => "fas fa-coins text-secondary",   // Litecoin
                "BINANCE:SHIBUSDT" => "fas fa-paw text-danger",       // Shiba Inu
                "BINANCE:USDCUSDT" => "fas fa-circle-dollar text-info", // USD Coin
                "BINANCE:TRXUSDT" => "fas fa-link text-danger",       // Tron
                "BINANCE:UNIUSDT" => "fas fa-atom text-purple",       // Uniswap

                // fallback
                _ => "fas fa-coins text-muted"
            };
        }

        private static readonly Dictionary<string, string> ForexIcons = new()
    {
        { "EUR/USD", "fas fa-euro-sign text-primary" },
        { "USD/JPY", "fas fa-yen-sign text-success" },
        { "GBP/USD", "fas fa-pound-sign text-warning" },
        { "AUD/USD", "fas fa-dollar-sign text-info" },
        { "USD/CAD", "fas fa-dollar-sign text-danger" }
    };

        public static string GetForexClass(string displaySymbol, string description)
        {
            if (string.IsNullOrWhiteSpace(displaySymbol) && string.IsNullOrWhiteSpace(description))
                return "fas fa-money-bill text-muted";

            // Normalize symbol: remove whitespace & uppercase
            var symbol = (displaySymbol ?? description ?? "").Trim().ToUpper();

            // Try exact match first
            if (ForexIcons.TryGetValue(symbol, out var icon))
                return icon;

            // Fallback: try to detect currency code in description
            if (description != null)
            {
                if (description.Contains("EUR")) return "fas fa-euro-sign text-primary";
                if (description.Contains("JPY")) return "fas fa-yen-sign text-success";
                if (description.Contains("GBP")) return "fas fa-pound-sign text-warning";
                if (description.Contains("AUD")) return "fas fa-dollar-sign text-info";
                if (description.Contains("CAD")) return "fas fa-dollar-sign text-danger";
            }

            return "fas fa-money-bill text-muted";
        }

        private static readonly Dictionary<string, string> CommodityIcons = new()
    {
        { "GOLD PER OUNCE (1OZ)", "fas fa-gem text-warning" },
        { "SILVER PER OUNCE (1OZ)", "fas fa-gem text-secondary" },
        { "BRENT CRUDE OIL", "fas fa-oil-well text-danger" },
        { "WTI CRUDE OIL", "fas fa-oil-well text-danger" }
    };

        public static string GetCommodityClass(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return "fas fa-chart-bar text-muted";

            var key = description.Trim().ToUpper();

            if (CommodityIcons.TryGetValue(key, out var icon))
                return icon;

            // Fallback generic icon
            return "fas fa-chart-bar text-muted";
        }
    }
}

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
                "NVDA" => "<svg viewBox=\"0 0 271.7 179.7\" xmlns=\"http://www.w3.org/2000/svg\" width=\"15\" height=\"10\"><path d=\"M101.3 53.6V37.4c1.6-.1 3.2-.2 4.8-.2 44.4-1.4 73.5 38.2 73.5 38.2S148.2 119 114.5 119c-4.5 0-8.9-.7-13.1-2.1V67.7c17.3 2.1 20.8 9.7 31.1 27l23.1-19.4s-16.9-22.1-45.3-22.1c-3-.1-6 .1-9 .4m0-53.6v24.2l4.8-.3c61.7-2.1 102 50.6 102 50.6s-46.2 56.2-94.3 56.2c-4.2 0-8.3-.4-12.4-1.1v15c3.4.4 6.9.7 10.3.7 44.8 0 77.2-22.9 108.6-49.9 5.2 4.2 26.5 14.3 30.9 18.7-29.8 25-99.3 45.1-138.7 45.1-3.8 0-7.4-.2-11-.6v21.1h170.2V0H101.3zm0 116.9v12.8c-41.4-7.4-52.9-50.5-52.9-50.5s19.9-22 52.9-25.6v14h-.1c-17.3-2.1-30.9 14.1-30.9 14.1s7.7 27.3 31 35.2M27.8 77.4s24.5-36.2 73.6-40V24.2C47 28.6 0 74.6 0 74.6s26.6 77 101.3 84v-14c-54.8-6.8-73.5-67.2-73.5-67.2z\" fill=\"#76b900\"/></svg>",
                "AMZN" => "fab fa-amazon text-warning",
                "TSLA" => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 1027.737 1024\" width=\"20\" height=\"20\"><path fill=\"#d81e06\" d=\"M514.491 1024l143.884-809.11c137.031 0 180.632 14.95 186.861 76.614 0 0 92.185-34.258 138.277-104.02C802.258 103.397 620.38 99.66 620.38 99.66L514.49 229.217 407.981 99.659s-181.879 3.738-363.134 87.825c46.092 69.762 138.277 104.02 138.277 104.02 6.229-61.665 49.207-76.613 185.616-76.613L514.49 1024z\"/><path fill=\"#d81e06\" d=\"M513.869 62.287c146.374-1.246 313.927 22.423 485.216 97.168 23.046-41.11 28.652-59.173 28.652-59.173C840.876 26.161 665.227.622 513.87 0 363.134.623 187.484 26.16 0 100.282c0 0 8.097 22.424 28.652 59.173C200.564 84.71 368.117 61.041 513.87 62.287z\"/></svg>",
                "META" => "fab fa-meta text-info",
                "NFLX" => "<svg height=\"20\" viewBox=\"124.528 16 262.944 480\" width=\"20\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"><linearGradient id=\"a\" gradientUnits=\"userSpaceOnUse\" x1=\"108.142\" x2=\"176.518\" y1=\"240.643\" y2=\"189.038\"><stop offset=\"0\" stop-color=\"#c20000\" stop-opacity=\"0\"/><stop offset=\"1\" stop-color=\"#9d0000\"/></linearGradient><linearGradient id=\"b\" x1=\"400.786\" x2=\"338.861\" xlink:href=\"#a\" y1=\"312.035\" y2=\"337.837\"/><path d=\"m216.398 16h-91.87v480c30.128-7.135 61.601-10.708 91.87-12.052z\" fill=\"#c20000\"/><path d=\"m216.398 16h-91.87v367.267c30.128-7.135 61.601-10.707 91.87-12.051z\" fill=\"url(#a)\"/><path d=\"m387.472 496v-480h-91.87v468.904c53.636 3.416 91.87 11.096 91.87 11.096z\" fill=\"#c20000\"/><path d=\"m387.472 496v-318.555h-91.87v307.459c53.636 3.416 91.87 11.096 91.87 11.096z\" fill=\"url(#b)\"/><path d=\"m387.472 496-171.074-480h-91.87l167.03 468.655c55.75 3.276 95.914 11.345 95.914 11.345z\" fill=\"#fa0000\"/></svg>",
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
                //"BINANCE:BNBUSDT" => "fab fa-binance text-warning",   // Binance Coin
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

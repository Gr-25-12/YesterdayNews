using System;
using System.Collections.Concurrent;


namespace FinanceServices.Utilities
{
    public static class FinanceConstants
    {
        public const string BaseUrl = "https://finnhub.io/api/v1/";

        public const string US = "US";
        public const string NYSE = "XNYS";
        public const string NASDAQ = "XNAS";
        public const string OANDA = "OANDA";
        public static string[] LargeSymbolsList
        {
            get
            {
                return _largeSymbolsList.Concat(usdForex.Keys.Concat(usdComm.Keys)).ToArray();
            }
            private set { }
        }
        public static string[] _largeSymbolsList { get; private set; } =
        {
            "NVDA", "MSFT", "AAPL", "GOOG", "GOOGL", "AMZN", "META", "AVGO",
            "NFLX", "TSLA", "BRK.B", "TSM", "V", "LLY", "MA", "JPM", "WMT",
            "ORCL", "XOM", "BRK.A",
            "BINANCE:BTCUSDT", "BINANCE:ETHUSDT", "BINANCE:XRPUSDT",
            "BINANCE:BNBUSDT", "BINANCE:SOLUSDT", "BINANCE:DOGEUSDT",
            "BINANCE:TRXUSDT", "BINANCE:ADAUSDT", "BINANCE:LINKUSDT",
            "BINANCE:SUIUSDT",
        };
        public static readonly string[] SmallSymbolsList =
        {
            "NVDA", "MSFT", "AAPL", "BRK.B", "TSM", "V",
                    "BINANCE:BTCUSDT", "BINANCE:ETHUSDT", "BINANCE:XRPUSDT",
                    "OANDA:EUR_USD","OANDA:USD_JPY", "OANDA:GBP_USD", "OANDA:XAU_USD","OANDA:XAG_USD",
        };
        public static readonly ConcurrentDictionary<string, string> cryptoDescriptionList = new ConcurrentDictionary<string, string>(new[]
        {
            new KeyValuePair<string, string>("BINANCE:BTCUSDT", "Bitcoin (BTC/USDT)"),
            new KeyValuePair<string, string>("BINANCE:ETHUSDT", "Etherium (USD/USDT)"),
            new KeyValuePair<string, string>("BINANCE:XRPUSDT", "Xrp / Ripple (XRP/USDT)"),
            new KeyValuePair<string, string>("BINANCE:BNBUSDT", "Binance (BNB/USDT)"),
            new KeyValuePair<string, string>("BINANCE:SOLUSDT", "Solana (SOL/USDT)"),
            new KeyValuePair<string, string>("BINANCE:DOGEUSDT", "Dodgecoin(DOGE/USDT)"),
            new KeyValuePair<string, string>("BINANCE:TRXUSDT",  "Tron (TRX/USDT)"),
            new KeyValuePair<string, string>("BINANCE:ADAUSDT", "Cardano (ADA/USDT)"),
            new KeyValuePair<string, string>("BINANCE:LINKUSDT", "Chainlink (LINK/USDT)"),
            new KeyValuePair<string, string>("BINANCE:SUIUSDT", "Sui (SUI/USDT)")
        });
        public static readonly ConcurrentDictionary<string, string> usdForex = new ConcurrentDictionary<string, string>(new[]
        {
            new KeyValuePair<string, string>("OANDA:EUR_USD", "Euro (EUR/USD)"),
            new KeyValuePair<string, string>("OANDA:USD_JPY", "Japanese Yen (USD/JPY)"), 
            new KeyValuePair<string, string>("OANDA:GBP_USD", "British Pound (GBP/USD)"),
            new KeyValuePair<string, string>("OANDA:USD_CHF", "Swiss Franc (USD/CHF)"),
            new KeyValuePair<string, string>("OANDA:AUD_USD", "Australian Dollar (AUD/USD)"), 
            new KeyValuePair<string, string>("OANDA:USD_CAD", "Canadian Dollar (USD/CAD)"),
            new KeyValuePair<string, string>("OANDA:USD_SEK", "Swedish Krona (USD/SEK)"), 
            new KeyValuePair<string, string>("OANDA:USD_NOK", "Norwegian Krone (USD/NOK)"),
            new KeyValuePair<string, string>("OANDA:USD_THB", "Thai Baht (USD/THB)") 
        });
        public static readonly ConcurrentDictionary<string, string> usdComm = new ConcurrentDictionary<string, string>(new[]
        {
            new KeyValuePair<string, string>("OANDA:XAU_USD", "Gold per Ounce (1oz)"),
            new KeyValuePair<string, string>("OANDA:XAG_USD", "Silver per Ounce (1oz)"),
            new KeyValuePair<string, string>("OANDA:BCO_USD", "Brent Crude Oil"),
            new KeyValuePair<string, string>("OANDA:WTICO_USD", "WTI Crude Oil"), 
        });
        public static readonly string[] SortedNasdaqReference =
        {
            "NVDA", "MSFT", "AAPL", "GOOG", "GOOGL", "AMZN", "META", "AVGO",
            "NFLX", "TSLA"
        };
        public static readonly string[] SortedNyseReference =
        {
            "BRK.A", "BRK.B", "TSM", "V", "LLY", "MA", "JPM", "WMT",
            "ORCL", "XOM", 
        };
        public static readonly string[] SortedCryptoReference =
        {
            "BINANCE:BTCUSDT", "BINANCE:ETHUSDT", "BINANCE:XRPUSDT",
            "BINANCE:BNBUSDT", "BINANCE:SOLUSDT", "BINANCE:DOGEUSDT",
            "BINANCE:TRXUSDT", "BINANCE:ADAUSDT", "BINANCE:LINKUSDT",
            "BINANCE:SUIUSDT"
        };
        public static readonly string[] SortedCurrenciesReference =
        {
            "OANDA:EUR_USD",
            "OANDA:USD_JPY",
            "OANDA:GBP_USD",
            "OANDA:USD_CHF",
            "OANDA:AUD_USD",
            "OANDA:USD_CAD",
            "OANDA:USD_SEK",
            "OANDA:USD_NOK", 
            "OANDA:USD_THB"
        };
        public static readonly string[] SortedCommoditiesReference =
        {
            "OANDA:XAU_USD",
            "OANDA:XAG_USD",
            "OANDA:BCO_USD",
            "OANDA:WTICO_USD"
        };
    }
}

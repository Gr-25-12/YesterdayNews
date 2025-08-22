using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceServices.Utilities
{
    public static class FinanceConstants
    {
        public const string BaseUrl = "https://finnhub.io/api/v1/";

        public const string US = "US";
        public const string NYSE = "XNYS";
        public const string NASDAQ = "XNAS";

        public static readonly string[] LargeSymbolsList =
        {
            "NVDA", "MSFT", "AAPL", "GOOG", "GOOGL", "AMZN", "META", "AVGO",
            "NFLX", "TSLA", "BRK.B", "TSM", "V", "LLY", "MA", "JPM", "WMT",
            "ORCL", "XOM", "BRK.A",
            "BINANCE:BTCUSDT", "BINANCE:ETHUSDT", "BINANCE:XRPUSDT",
            "BINANCE:BNBUSDT", "BINANCE:SOLUSDT", "BINANCE:DOGEUSDT",
            "BINANCE:TRXUSDT", "BINANCE:ADAUSDT", "BINANCE:LINKUSDT",
            "BINANCE:HYPEUSDT"
        };
        public static readonly string[] SmallSymbolsList =
        {
            "NVDA", "MSFT", "AAPL", "BRK.B", "TSM", "V",
                    "BINANCE:BTCUSDT", "BINANCE:ETHUSDT", "BINANCE:XRPUSDT"
        };
    }
}

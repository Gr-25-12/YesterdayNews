using NuGet.Common;
using YesterdayNews.Models.API;
using YesterdayNews.Services.IServices;
using static System.Net.WebRequestMethods;

namespace YesterdayNews.Services
{
    public class FinanceApiServices : IFinanceApiServices
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _finnhubBaseUrl = "https://finnhub.io/api/v1/";

        private List<UsStock>? NasdaqList;
        private List<UsStock>? NyseList;
        public FinanceApiServices(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = "&token=" + config["Finnhub:ApiKey"];
        }
        
        public async Task<List<UsStock>> GetNasdaqStockList()
        {
            if (NasdaqList != null) return NasdaqList;

            string Nasdaq = "stock/symbol?exchange=US&mic=XNAS";
            var url = $"{_finnhubBaseUrl}{Nasdaq}{_apiKey}";
            NasdaqList = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
            return NasdaqList;
        }
        public async Task<List<UsStock>> GetNyseStockList()
        {
            if (NyseList != null) return NyseList;

            string Nyse = "stock/symbol?exchange=US&mic=XNYS";
            var url = $"{_finnhubBaseUrl}{Nyse}{_apiKey}";
            NyseList = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
            return NyseList;
        }
        public async Task<StockQuote> GetStockQuote(string tickerSymbol)
        {
            string stockQuote = $"/quote?symbol={tickerSymbol}";
            var url = $"{_finnhubBaseUrl}{stockQuote}{_apiKey}";
            var quote = await _httpClient.GetFromJsonAsync<StockQuote>(url);
            return quote;
        }
    }
}

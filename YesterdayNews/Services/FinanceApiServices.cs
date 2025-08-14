using Microsoft.CodeAnalysis.Elfie.Model;
using YesterdayNews.Models.API;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;


namespace YesterdayNews.Services
{
    public class FinanceApiServices : IFinanceApiServices
    {
        private readonly HttpClient _httpClient;
        private readonly string _finnhubApiKey;
        private readonly string _financialModellingApiKey;
        private readonly string _finnhubBaseUrl = "https://finnhub.io/api/v1/";
        private readonly string _financialmodelingUrl = "https://financialmodelingprep.com/stable/";

        private List<UsStock>? NasdaqList;
        private List<UsStock>? NyseList;
        public FinanceApiServices(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _finnhubApiKey = "&token=" + config["Finnhub:ApiKey"];
            _financialModellingApiKey = "" + config["Financialmodeling:ApiKey"];
        }

        public async Task<MarketsVM> GetMarketVM()
        {
            var model = new MarketsVM();

            var stocks = await GetNasdaqStockList();
            string[] Top3Symbols = { "NVDA", "MSFT", "AAPL" };
            var topStocks = stocks
                .Where(s => Top3Symbols.Contains(s.Symbol))
                .ToList();

            foreach (var stock in topStocks)
            {
                if (string.IsNullOrWhiteSpace(stock.Symbol))
                    continue;

                var quote = await GetStockQuote(stock.Symbol);

                model.StockInfo[stock.Symbol] = stock;
                model.StockPrices[stock.Symbol] = quote;
            }

            return model;
        }

        public async Task<string> UpdateRatesAndPrices()
        {
            throw new NotImplementedException();
        }

        public async Task<List<UsStock>> GetNasdaqStockList()
        {
            if (NasdaqList != null) return NasdaqList;

            string Nasdaq = "stock/symbol?exchange=US&mic=XNAS";
            var url = $"{_finnhubBaseUrl}{Nasdaq}{_finnhubApiKey}";
            NasdaqList = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
            return NasdaqList;
        }
        public async Task<List<UsStock>> GetNyseStockList()
        {
            if (NyseList != null) return NyseList;

            string Nyse = "stock/symbol?exchange=US&mic=XNYS";
            var url = $"{_finnhubBaseUrl}{Nyse}{_finnhubApiKey}";
            NyseList = await _httpClient.GetFromJsonAsync<List<UsStock>>(url) ?? new List<UsStock>();
            return NyseList;
        }
        public async Task<StockQuote> GetStockQuote(string tickerSymbol)
        {
            string stockQuote = $"/quote?symbol={tickerSymbol}";
            var url = $"{_finnhubBaseUrl}{stockQuote}{_finnhubApiKey}";
            var quote = await _httpClient.GetFromJsonAsync<StockQuote>(url);
            return quote;
        }
        public async Task<string> GetForexQuotes(string exchangeName)
        {
            throw new NotImplementedException();
        }
        public async Task<string> GetCryptoQuotes(string exchangeName)
        {
            throw new NotImplementedException();
        }
        public async Task<CompanyProfile> GetCompanyProfile(string tickerSymbol)
        {
            string profileLink = $"/stock/profile2?symbol={tickerSymbol}";
            var url = $"{_finnhubBaseUrl}{profileLink}{_finnhubApiKey}";
            var profile = await _httpClient.GetFromJsonAsync<CompanyProfile>(url);
            return profile;
        }

        public async Task<string> GetCompanyFinancials(string tickerSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetCompanyNews(string tickerSymbol)
        {
            throw new NotImplementedException();
        }
        public async Task<string> GetMarketStatus(string exchangeName)
        {
            throw new NotImplementedException();
        }
        public async Task<string> GetMarketNews(string marketType)
        {
            throw new NotImplementedException();
        }

      
    }
}

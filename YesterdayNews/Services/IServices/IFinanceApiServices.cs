using YesterdayNews.Models.API;
using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{
    public interface IFinanceApiServices
    {
        Task<MarketsVM> GetMarketVM();
        Task<string> UpdateRatesAndPrices();
        Task<List<UsStock>> GetNasdaqStockList();
        Task<List<UsStock>> GetNyseStockList();
        Task<List<string>> GetSP500Index();
        Task<StockQuote> GetStockQuote(string tickerSymbol);
        Task<string> GetForexQuotes(string exchangeName);
        Task<string> GetCryptoQuotes(string exchangeName);
        Task<CompanyProfile> GetCompanyProfile(string tickerSymbol);
        Task<string> GetCompanyFinancials(string tickerSymbol);
        Task<string> GetCompanyNews(string tickerSymbol);
        Task<string> GetMarketStatus(string exchangeName);
        Task<string> GetMarketNews(string marketType);

    }
}

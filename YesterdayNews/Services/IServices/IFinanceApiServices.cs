using YesterdayNews.Models.API;

namespace YesterdayNews.Services.IServices
{
    public interface IFinanceApiServices
    {
        Task<List<UsStock>> GetNasdaqStockList();
        Task<List<UsStock>> GetNyseStockList();
        Task<StockQuote> GetStockQuote(string tickerSymbol);
    }
}

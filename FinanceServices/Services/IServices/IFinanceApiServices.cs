
using FinanceServices.Models;

namespace FinanceServices.Services.IServices
{
    public interface IFinanceApiServices
    {
        MarketDto GetMarketsModel(string[] symbols = null);
        string[] GetSmallSymbolList();
        string GetMarketStatus(string exchange);
    }
}

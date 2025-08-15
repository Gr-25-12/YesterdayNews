using Microsoft.CodeAnalysis.Elfie.Model;
using YesterdayNews.Models.API;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;


namespace YesterdayNews.Services
{
    public class FinanceApiServices : IFinanceApiServices
    {
        
        public FinanceApiServices()
        {
            
        }

        public async Task<MarketsVM> GetMarketVM()
        {
            var model = new MarketsVM();
            var stocks = FinnhubBackgroundService.NasdaqList;
            string[] Top3Symbols = { "NVDA", "MSFT", "AAPL" };
            //var topStocks = stocks
            //    .Where(s => Top3Symbols.Contains(s.Symbol))
            //    .ToList();

            foreach (var stock in stocks)
            {
                if (string.IsNullOrWhiteSpace(stock.Symbol))
                    continue;
                var quote = FinnhubBackgroundService.GetCachedStockQuote(stock.Symbol);
                if(quote != null)
                    model.StockPrices[stock.Symbol] = quote;

                model.StockInfo[stock.Symbol] = stock;
            }

            return model;
        }      
    }
}

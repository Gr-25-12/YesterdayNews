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

        public async Task<MarketsVM> GetMarketsVM()
        {
            var model = new MarketsVM();

            //Nasdaq
            var nasdaq = FinnhubBackgroundService.NasdaqList;
            foreach (var stock in nasdaq)
            {
                if (string.IsNullOrWhiteSpace(stock.Symbol))
                    continue;
                var quote = FinnhubBackgroundService.GetCachedStockQuote(stock.Symbol);
                if(quote != null)
                    model.NasdaqStockPrices[stock.Symbol] = quote;

                model.NasdaqStockInfo[stock.Symbol] = stock;
            }
            //NYSE
            var nyse = FinnhubBackgroundService.NyseList;

            foreach (var stock in nyse)
            {
                if (string.IsNullOrWhiteSpace(stock.Symbol))
                    continue;
                var quote = FinnhubBackgroundService.GetCachedStockQuote(stock.Symbol);
                if (quote != null)
                    model.NyseStockPrices[stock.Symbol] = quote;

                model.NyseStockInfo[stock.Symbol] = stock;
            }
            //CRYPTO
            model.CryptoPrices = FinnhubBackgroundService.CryptoQuotes;
            return model;
        } 

    }
}

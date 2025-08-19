using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.IdentityModel.Tokens;
using YesterdayNews.Models.API;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;


namespace YesterdayNews.Services
{
    public class FinanceApiServices : IFinanceApiServices
    {

        public MarketsVM GetMarketsVM(string[] symbols = null)
        {
            string[] stockSymbols = FinnhubBackgroundService.StockQuotes.Keys.ToArray();
            string[] cryptoSymbols = FinnhubBackgroundService.CryptoQuotes.Keys.ToArray();
            string[] allSymbols = stockSymbols.Concat(cryptoSymbols).ToArray();

            MarketsVM model = new MarketsVM();
            var symbolsToUse = (symbols == null || symbols.Length == 0) ? allSymbols : symbols;

            if (symbolsToUse != null)
            {
                foreach (var symbol in symbolsToUse)
                {
                        SetMarketVM(ref model, symbol);
                }
            }
            return model;
        }

        private void SetMarketVM(ref MarketsVM model, string symbol)
        {
            var stockinfo = FinnhubBackgroundService.GetCachedUsStock(symbol);

            if (stockinfo != null)
            { 
                var quote = FinnhubBackgroundService.GetCachedStockQuote(symbol);
                if (quote != null)
                {
                    if (stockinfo.Mic == FinnhubBackgroundService.NASDAQ)
                    {
                        model.NasdaqStockPrices[symbol] = quote;
                        model.NasdaqStockInfo[symbol] = stockinfo;
                    }
                    else if (stockinfo.Mic == FinnhubBackgroundService.NYSE)
                    {
                        model.NyseStockPrices[symbol] = quote;
                        model.NyseStockInfo[symbol] = stockinfo;
                    }
                }
            }
            var cryptoInfo = FinnhubBackgroundService.GetCachedCryptoQuote(symbol);
            if (cryptoInfo != null)
            {
                model.CryptoPrices[symbol] = cryptoInfo;
            }
        }
    }
}

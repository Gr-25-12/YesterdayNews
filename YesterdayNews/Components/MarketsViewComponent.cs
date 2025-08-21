using Microsoft.AspNetCore.Mvc;
using FinanceServices.Services.IServices;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Components
{
    public class MarketsViewComponent : ViewComponent
    {
        private readonly IFinanceApiServices _financeApiServices;
        private readonly ICategoryService _categoryService;
        public MarketsViewComponent(IFinanceApiServices financeApiServices, ICategoryService categoryService)
        {
            _financeApiServices = financeApiServices;
            _categoryService = categoryService;
        }
        public IViewComponentResult Invoke()
        {
             string[] stocksToDisplay = { "NVDA", "MSFT", "AAPL", "BRK.B", "TSM", "V",
                    "BINANCE:BTCUSDT", "BINANCE:ETHUSDT", "BINANCE:XRPUSDT"};

        var model =  _financeApiServices.GetMarketsModel(stocksToDisplay);
            return View(model);
        }
    }
}

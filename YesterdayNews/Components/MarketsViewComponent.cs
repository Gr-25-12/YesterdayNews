using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Services;
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
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _financeApiServices.GetMarketVM();
            return View(model);
        }
    }
}

using YesterdayNews.Models.API;
using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{
    public interface IFinanceApiServices
    {
        Task<MarketsVM> GetMarketsVM();
    }
}

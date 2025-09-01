namespace YesterdayNews.Services.IServices
{
    public interface IFinanceEventHandler
    {
        Task HandlePriceUpdate();
        Task HandleMarketStatusApiError(string error);
    }
}

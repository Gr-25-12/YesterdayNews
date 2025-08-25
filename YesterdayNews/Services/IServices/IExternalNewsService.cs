using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{
    public interface IExternalNewsService
    {
        Task<List<ExternalNewsVM>> GetTopNewsAsync();
    }
}

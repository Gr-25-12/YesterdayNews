using Microsoft.AspNetCore.Mvc;
using YesterdayNews.Models.ViewModels;

namespace YesterdayNews.Services.IServices
{
    public interface IExternalNewsService
    {
        Task<List<ExternalNewsVM>> GetTopNewsAsync([FromServices] IWebHostEnvironment env);
    }
}

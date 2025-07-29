using YesterdayNews.Models.Db;

namespace YesterdayNews.Services.IServices
{
    public interface IFileServices
    {
        Task<string> UploadFileToContainer(IFormFile file);
        Task DeleteFileFromContainer(string fileUrl);
    }
}

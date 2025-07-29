using Azure.Storage.Blobs;
using YesterdayNews.Models.Db;
using YesterdayNews.Services;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class FileServices : IFileServices
    {
        private readonly IConfiguration _configuration;
        public FileServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> UploadFileToContainer(IFormFile file)
        {
            string blobConnectionString = _configuration["AzureBlobStorage"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
            string containerName = _configuration["AzureBlobContainerName"];
            BlobContainerClient containerClient =
            blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = containerClient.GetBlobClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);

            }
            return blobClient.Uri.ToString();
        }
    }
}



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

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

            var blobClient = containerClient.GetBlobClient(uniqueFileName);


            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);

            }
            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileFromContainer(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return;

            
                string blobConnectionString = _configuration["AzureBlobStorage"];
                BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
                string containerName = _configuration["AzureBlobContainerName"];
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                
                Uri uri = new Uri(fileUrl);
                string blobName = uri.Segments.Last();

                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();
            
           
        }
    }
}



using AkouoApi.Models;

namespace AkouoApi.Services
{
    public interface IS3Service
    {
        Task<S3Response> ListObjectsAsync(string folder = "");
        Task<S3Response> ReadObjectDataAsync(string keyName, string folder = "", bool forWrite = false);
        Task<bool> FileExistsAsync(string fileName, string folder = "");
        S3Response SignedUrlForGet(string fileName, string folder, string contentType);
        Task<S3Response> MakePublic(string fileName, string folder = "");
        string ObjectUrl(string fileName, string folder = "");
    }
}

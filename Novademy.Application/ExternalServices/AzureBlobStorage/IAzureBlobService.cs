using Microsoft.AspNetCore.Http;

namespace Novademy.Application.ExternalServices.AzureBlobStorage;

public interface IAzureBlobService
{
    Task<string> UploadFileAsync(IFormFile file);
    Task<bool> DeleteFileAsync(string fileName);
}
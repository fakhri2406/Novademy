using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Novademy.Application.ExternalServices.AzureBlobStorage;

public class AzureBlobService : IAzureBlobService
{
    private readonly BlobContainerClient _containerClient;
    private readonly AzureBlobOptions _options;
    
    public AzureBlobService(IOptions<AzureBlobOptions> options)
    {
        _options = options.Value;
        var connectionString = $"DefaultEndpointsProtocol=https;AccountName={_options.AccountName};AccountKey={_options.AccountKey};EndpointSuffix=core.windows.net";
        var blobServiceClient = new BlobServiceClient(connectionString);
        
        _containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }
    
    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("File is empty", nameof(file));
        }
        
        var uniqueFileName = $"{file.FileName}_{Guid.NewGuid()}";
        var blobClient = _containerClient.GetBlobClient(uniqueFileName);
        
        await using var stream = file.OpenReadStream();
        
        await blobClient.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = file.ContentType
        });
        
        return blobClient.Uri.ToString();
    }
    
    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        var response = await blobClient.DeleteIfExistsAsync();
        return response.Value;
    }
}
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Novademy.Application.ExternalServices.Cloudinary;

public class CloudinaryService : ICloudinaryService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    
    public CloudinaryService(CloudinaryDotNet.Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }
    
    public async Task<ImageUploadResult> UploadImageAsync(IFormFile image, string folder)
    {
        using var stream = image.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(image.FileName, stream),
            Folder = folder,
            PublicId = Guid.NewGuid().ToString()
        };
        
        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
        {
            throw new Exception(result.Error.Message);
        }
        
        return result;
    }
    
    public async Task<VideoUploadResult> UploadVideoAsync(IFormFile video, string folder)
    {
        using var stream = video.OpenReadStream();
        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(video.FileName, stream),
            Folder = folder,
            PublicId = Guid.NewGuid().ToString()
        };
        
        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
        {
            throw new Exception(result.Error.Message);
        }
        
        return result;
    }
    
    public async Task<DeletionResult> DeleteFileAsync(string fileUrl, ResourceType resourceType)
    {
        if (string.IsNullOrEmpty(fileUrl))
        {
            throw new ArgumentException("File URL cannot be null or empty.", nameof(fileUrl));
        }
        
        string publicId = ExtractPublicIdFromUrl(fileUrl);
        
        var deletionParams = new DeletionParams(publicId)
        {
            ResourceType = resourceType
        };
        
        var result = await _cloudinary.DestroyAsync(deletionParams);
        if (result.Error != null)
        {
            throw new Exception(result.Error.Message);
        }
        
        return result;
    }
    
    private string ExtractPublicIdFromUrl(string fileUrl)
    {
        Uri uri = new Uri(fileUrl);
        string path = uri.AbsolutePath;
        
        string[] segments = path.Split('/');
        string lastSegment = segments[^1];
        string publicIdWithExtension = lastSegment.Split('.')[0];
        
        int uploadIndex = Array.IndexOf(segments, "upload");
        if (uploadIndex == -1 || uploadIndex >= segments.Length - 1)
        {
            throw new ArgumentException("Invalid Cloudinary URL format. Expected 'upload' in the path.", nameof(fileUrl));
        }
        
        string[] publicIdSegments = segments[(uploadIndex + 1)..^0];
        publicIdSegments[^1] = publicIdWithExtension;
        string publicId = string.Join("/", publicIdSegments);
        
        return publicId;
    }
}
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Novademy.Application.Cloudinary;

public interface IMediaUpload
{
    Task<ImageUploadResult> UploadImageAsync(IFormFile image, string folder);
    Task<VideoUploadResult> UploadVideoAsync(IFormFile video, string folder);
}
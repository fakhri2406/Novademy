using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IPackageRepository
{
    Task<Package> CreatePackageAsync(Package package, IFormFile image);
    Task<ImageUploadResult> UploadImageAsync(IFormFile file);
    Task<IEnumerable<Package>> GetAllPackagesAsync();
    Task<Package?> GetPackageByIdAsync(Guid id);
    Task<Package?> UpdatePackageAsync(Package package);
    Task DeletePackageAsync(Guid id);
}
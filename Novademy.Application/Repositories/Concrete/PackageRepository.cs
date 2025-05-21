using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data.EFCore;
using Novademy.Application.ExternalServices.Cloudinary;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class PackageRepository : IPackageRepository
{
    private readonly AppDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;
    
    public PackageRepository(AppDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }
    
    public async Task<Package> CreatePackageAsync(Package package, IFormFile? image)
    {
        if (image is not null)
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(image, "packages");
            package.ImageUrl = uploadResult.SecureUrl.ToString();
        }

        package.CreatedAt = DateTime.UtcNow;
        package.UpdatedAt = DateTime.UtcNow;

        _context.Packages.Add(package);
        await _context.SaveChangesAsync();

        return package;
    }
    
    public async Task<IEnumerable<Package>> GetAllPackagesAsync()
    {
        return await _context.Packages
            .Include(p => p.Courses)
            .ToListAsync();
    }
    
    public async Task<Package?> GetPackageByIdAsync(Guid id)
    {
        var package = await _context.Packages
            .Include(p => p.Courses)
            .SingleOrDefaultAsync(p => p.Id == id);
        
        if (package == null)
        {
            throw new KeyNotFoundException("Invalid Package ID.");
        }
        
        return package;
    }
    
    public async Task<Package?> UpdatePackageAsync(Package package, IFormFile? image)
    {
        if (image is not null)
        {
            var imageUrl = package.ImageUrl;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _cloudinaryService.DeleteFileAsync(imageUrl, ResourceType.Image);
            }
            var uploadResult = await _cloudinaryService.UploadImageAsync(image, "packages");
            package.ImageUrl = uploadResult.SecureUrl.ToString();
        }

        package.UpdatedAt = DateTime.UtcNow;

        _context.Packages.Update(package);
        await _context.SaveChangesAsync();

        return package;
    }
    
    public async Task DeletePackageAsync(Guid id)
    {
        var package = await _context.Packages
            .Include(p => p.Courses)
            .SingleOrDefaultAsync(p => p.Id == id);
        
        if (package == null)
        {
            throw new KeyNotFoundException("Invalid Package ID.");
        }
        
        var imageUrl = package.ImageUrl;
        if (!string.IsNullOrEmpty(imageUrl))
        {
            await _cloudinaryService.DeleteFileAsync(imageUrl, ResourceType.Image);
        }
        
        _context.Packages.Remove(package);
        await _context.SaveChangesAsync();
    }
}
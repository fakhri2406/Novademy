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
    
    #region Create
    
    public async Task<Package> CreateAsync(Package package, IFormFile? image)
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
    
    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Package>> GetAllAsync()
    {
        return await _context.Packages
            .Include(p => p.Courses)
            .ToListAsync();
    }
    
    public async Task<Package?> GetByIdAsync(Guid id)
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
    
    #endregion
    
    #region Update
    
    public async Task<Package?> UpdateAsync(Package package, IFormFile? image)
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
    
    #endregion
    
    #region Delete
    
    public async Task DeleteAsync(Guid id)
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
    
    #endregion
}
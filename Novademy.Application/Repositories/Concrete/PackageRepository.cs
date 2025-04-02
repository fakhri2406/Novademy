using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class PackageRepository : IPackageRepository
{
    private readonly AppDbContext _context;
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    
    public PackageRepository(AppDbContext context, CloudinaryDotNet.Cloudinary cloudinary)
    {
        _context = context;
        _cloudinary = cloudinary;
    }
    
    #region Create

    public async Task<Package> CreatePackageAsync(Package package, IFormFile image)
    {
        package.Id = Guid.NewGuid();
        
        if (image is not null)
        {
            var uploadResult = await UploadImageAsync(image);
            package.ImageUrl = uploadResult.SecureUrl.ToString();
        }
        
        _context.Packages.Add(package);
        await _context.SaveChangesAsync();
        
        return package;
    }
    
    public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "packages",
            PublicId = Guid.NewGuid().ToString()
        };
        
        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
        {
            throw new Exception(result.Error.Message);
        }
        
        return result;
    }

    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Package>> GetAllPackagesAsync()
    {
        return await _context.Packages
            .Include(p => p.Courses)
            .ToListAsync();
    }
    
    public async Task<Package?> GetPackageByIdAsync(Guid id)
    {
        if (!_context.Packages.Any(p => p.Id == id))
        {
            throw new KeyNotFoundException("Invalid Package ID.");
        }
        return await _context.Packages
            .Include(p => p.Courses)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    #endregion
    
    #region Update
    
    public async Task<Package?> UpdatePackageAsync(Package package)
    {
        _context.Packages.Update(package);
        await _context.SaveChangesAsync();
        return package;
    }
    
    #endregion
    
    #region Delete
    
    public async Task DeletePackageAsync(Guid id)
    {
        var package = await _context.Packages.FindAsync(id);
        
        if (package is null)
        {
            throw new KeyNotFoundException("Invalid Package ID.");
        }
        
        _context.Packages.Remove(package);
        await _context.SaveChangesAsync();
    }
    
    #endregion
}
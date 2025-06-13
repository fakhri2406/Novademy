using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data.EFCore;
using Novademy.Application.ExternalServices.Cloudinary;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;
    
    public UserRepository(AppDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }
    
    #region Read

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.Include(u => u.Role).ToListAsync();
    }
    
    public async Task<User> GetByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        
        return user;
    }

    #endregion
    
    #region Update
    
    public async Task<User> UpdateAsync(User user, IFormFile? profilePicture)
    {
        if (profilePicture is not null)
        {
            var imageUrl = user.ProfilePictureUrl;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _cloudinaryService.DeleteFileAsync(imageUrl, ResourceType.Image);
            }
            var uploadResult = await _cloudinaryService.UploadImageAsync(profilePicture, "users");
            user.ProfilePictureUrl = uploadResult.SecureUrl.ToString();
        }
        
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        
        return user;
    }
    
    #endregion
    
    #region Delete
    
    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException("Invalid User ID.");
        }
        
        var profilePictureUrl = user.ProfilePictureUrl;
        if (!string.IsNullOrEmpty(profilePictureUrl))
        {
            await _cloudinaryService.DeleteFileAsync(profilePictureUrl, ResourceType.Image);
        }
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
    
    #endregion
}
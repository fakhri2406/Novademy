using CloudinaryDotNet.Actions;
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

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.Include(u => u.Role).ToListAsync();
    }
    
    public async Task<User> GetUserByIdAsync(Guid userId)
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
    
    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task DeleteUserAsync(Guid id)
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
}
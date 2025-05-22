using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data.EFCore;
using Novademy.Application.ExternalServices.Cloudinary;
using Novademy.Application.Helpers;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;
    
    public AuthRepository(AppDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }
    
    #region Register
    
    public async Task<User> RegisterUserAsync(User user, IFormFile? profilePicture)
    {
        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
        {
            throw new ArgumentException("Username already exists.");
        }
        
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
        {
            throw new ArgumentException("Email already exists.");
        }
        
        if (await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber))
        {
            throw new ArgumentException("Phone number already exists.");
        }
        
        if (await _context.Users.AnyAsync(u => u.RoleId == 1) && user.RoleId == 1)
        {
            throw new ArgumentException("Admin already exists.");
        }
        
        user.Password = Hasher.HashPassword($"{user.Password}{user.Salt}");
        
        if (profilePicture is not null)
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(profilePicture, "users");
            user.ProfilePictureUrl = uploadResult.SecureUrl.ToString();
        }
        
        user.EmailVerificationCode = new Random().Next(1000, 10000).ToString();
        user.EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(15);
        user.IsEmailVerified = false;
        
        user.RegisteredAt = DateTime.UtcNow;
        user.LastLoginAt = DateTime.UtcNow;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return user;
    }
    
    #endregion
    
    #region Login
    
    public async Task<User> LoginUserAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Username == username);
        
        if (user is null)
        {
            throw new KeyNotFoundException("Invalid username or password.");
        }
        
        var hashedPassword = Hasher.HashPassword($"{password}{user.Salt}");
        if (hashedPassword != user.Password)
        {
            throw new KeyNotFoundException("Invalid password.");
        }
        
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return user;
    }
    
    #endregion
    
    #region Refresh
    
    public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }
    
    public async Task<RefreshToken> GetRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(r => r.User)
            .ThenInclude(u => u!.Role)
            .SingleOrDefaultAsync(r => r.Token == token);
        
        if (refreshToken is null)
        {
            throw new KeyNotFoundException("Invalid refresh token.");
        }
        
        return refreshToken;
    }
    
    public async Task RemoveRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .SingleOrDefaultAsync(r => r.Token == token);
        
        if (refreshToken is null)
        {
            throw new KeyNotFoundException("Invalid refresh token.");
        }
        
        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
    }
    
    #endregion
    
    #region Logout
    
    public async Task RemoveAllRefreshTokensAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId)
            .ToListAsync();
        
        if (!tokens.Any())
        {
            throw new KeyNotFoundException("No refresh tokens found for the user.");
        }
        
        _context.RefreshTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }
    
    #endregion
    
    #region Get
    
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
    
    #endregion
    
    #region Update
    
    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    #endregion
}
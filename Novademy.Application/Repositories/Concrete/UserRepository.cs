using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data.EFCore;
using Novademy.Application.Helpers;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Cloudinary;

namespace Novademy.Application.Repositories.Concrete;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly IMediaUpload _mediaUpload;
    
    public UserRepository(AppDbContext context, IMediaUpload mediaUpload)
    {
        _context = context;
        _mediaUpload = mediaUpload;
    }
    
    #region Register
    
    public async Task<User> RegisterUserAsync(User user, IFormFile? profilePicture)
    {
        user.Id = Guid.NewGuid();
        user.Salt = Guid.NewGuid().ToString();
        user.Password = Hasher.HashPassword($"{user.Password}{user.Salt}");
        
        if (profilePicture is not null)
        {
            var uploadResult = await _mediaUpload.UploadImageAsync(profilePicture, "user_profiles");
            user.ProfilePictureUrl = uploadResult.SecureUrl.ToString();
        }
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        return user;
    }
    
    #endregion
    
    #region Login

    public async Task<User> LoginUserAsync(string username, string password)
    {
        var loggedInUser = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username);
        
        if (loggedInUser is null)
        {
            throw new KeyNotFoundException("Invalid username or password.");
        }
        
        var hashedPassword = Hasher.HashPassword($"{password}{loggedInUser.Salt}");
        if (hashedPassword != loggedInUser.Password)
        {
            throw new KeyNotFoundException("Invalid password.");
        }
        
        return loggedInUser;
    }
    
    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    #endregion
    
    #region Refresh
    
    public async Task CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }
    
    public async Task<RefreshToken> GetRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(r => r.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(t => t.Token == token);
    
        if (refreshToken is null)
        {
            throw new KeyNotFoundException("Invalid refresh token.");
        }
        return refreshToken;
    }
    
    public async Task RemoveRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
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
        var tokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
        if (tokens is null || !tokens.Any())
        {
            throw new KeyNotFoundException("No refresh tokens found for the user.");
        }
        _context.RefreshTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }
    
    #endregion
}
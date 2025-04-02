using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IUserRepository
{
    Task<User> RegisterUserAsync(User user, IFormFile profilePicture);
    Task<ImageUploadResult> UploadProfilePictureAsync(IFormFile file);
    Task<User> LoginUserAsync(string username, string password);
    Task<User> UpdateUserAsync(User user);
    Task CreateRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken> GetRefreshTokenAsync(string token);
    Task RemoveRefreshTokenAsync(string token);
    Task RemoveAllRefreshTokensAsync(Guid userId);
}
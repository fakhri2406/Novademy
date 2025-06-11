using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IAuthRepository
{
    Task<User> RegisterAsync(User user, IFormFile? profilePicture);
    Task<User> LoginAsync(string username, string password);
    Task CreateRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken> GetRefreshTokenAsync(string token);
    Task RemoveRefreshTokenAsync(string token);
    Task RemoveAllRefreshTokensAsync(Guid userId);
}
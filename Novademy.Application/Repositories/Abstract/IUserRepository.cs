using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IUserRepository
{
    Task<User> RegisterUserAsync(User user);
    Task<User> LoginUserAsync(string username, string password);
    Task<User> UpdateUserAsync(User user);
    Task CreateRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken> GetRefreshTokenAsync(string token);
    Task RemoveRefreshTokenAsync(string token);
    Task RemoveAllRefreshTokensAsync(Guid userId);
}
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> GetUserByIdAsync(Guid userId);
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(Guid userId);
}
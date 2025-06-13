using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> GetByIdAsync(Guid userId);
    Task<User> UpdateAsync(User user, IFormFile? profilePicture);
    Task DeleteAsync(Guid userId);
}
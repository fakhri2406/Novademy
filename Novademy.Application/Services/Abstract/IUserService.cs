using Novademy.Contracts.Requests.User;
using Novademy.Contracts.Responses.User;

namespace Novademy.Application.Services.Abstract;

public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllAsync();
    Task<UserResponse> GetByIdAsync(Guid userId);
    Task<UserResponse> UpdateAsync(Guid userId, UpdateUserRequest request);
    Task DeleteAsync(Guid userId);
}
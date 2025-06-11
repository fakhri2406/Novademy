using Novademy.Application.Mapping;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.User;
using Novademy.Contracts.Responses.User;

namespace Novademy.Application.Services.Concrete;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return users.Select(u => u.MapToUserResponse());
    }
    
    public async Task<UserResponse> GetByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        return user.MapToUserResponse();
    }
    
    public async Task<UserResponse> UpdateAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        
        user.Username = request.Username;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.Group = request.Group;
        user.Sector = request.Sector;
        
        var updatedUser = await _userRepository.UpdateUserAsync(user);
        return updatedUser.MapToUserResponse();
    }
    
    public async Task DeleteAsync(Guid userId)
    {
        await _userRepository.DeleteUserAsync(userId);
    }
}
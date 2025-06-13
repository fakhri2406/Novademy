using FluentValidation;
using Novademy.Application.Mapping;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.User;
using Novademy.Contracts.Responses.User;

namespace Novademy.Application.Services.Concrete;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<UpdateUserRequest> _updateValidator;

    public UserService(IUserRepository userRepository, IValidator<UpdateUserRequest> updateValidator)
    {
        _userRepository = userRepository;
        _updateValidator = updateValidator;
    }
    
    #region Read
    
    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => u.MapToUserResponse());
    }
    
    public async Task<UserResponse> GetByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user.MapToUserResponse();
    }
    
    #endregion
    
    #region Update
    
    public async Task<UserResponse> UpdateAsync(Guid userId, UpdateUserRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        
        var user = await _userRepository.GetByIdAsync(userId);
        
        user.Username = request.Username;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.Group = request.Group;
        user.Sector = request.Sector;
        
        var updatedUser = await _userRepository.UpdateAsync(user, request.ProfilePicture);
        return updatedUser.MapToUserResponse();
    }
    
    #endregion
    
    #region Delete
    
    public async Task DeleteAsync(Guid userId)
    {
        await _userRepository.DeleteAsync(userId);
    }
    
    #endregion
}
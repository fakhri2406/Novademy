using Novademy.Contracts.Requests;
using Novademy.Application.Models;

namespace Novademy.API.Mapping;

public static class ContractMapping
{
    public static User MapToUser(this RegisterRequest request)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
            RoleId = request.RoleId
        };
    }
}
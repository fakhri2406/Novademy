using Novademy.Application.Models;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Requests.Course;
using Novademy.Contracts.Responses.Course;

namespace Novademy.API.Mapping;

public static class ContractMapping
{
    public static User MapToUser(this RegisterRequest request)
    {
        return new User
        {
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
            RoleId = request.RoleId
        };
    }

    public static Course MapToCourse(this CreateCourseRequest request)
    {
        return new Course()
        {
            Title = request.Title,
            Description = request.Description,
        };
    }

    public static CourseResponse MapToCourseResponse(this Course course)
    {
        return new CourseResponse()
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
        };
    }
}
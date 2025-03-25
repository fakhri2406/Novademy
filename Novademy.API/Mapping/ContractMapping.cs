using Novademy.Application.Models;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Requests.Course;
using Novademy.Contracts.Requests.Lesson;
using Novademy.Contracts.Requests.Package;
using Novademy.Contracts.Responses.Course;
using Novademy.Contracts.Responses.Lesson;
using Novademy.Contracts.Responses.Package;

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
    
    public static Lesson MapToLesson(this CreateLessonRequest request)
    {
        return new Lesson()
        {
            Title = request.Title,
            Description = request.Description,
            VideoUrl = request.VideoUrl,
            CourseId = request.CourseId,
        };
    }
    
    public static LessonResponse MapToLessonResponse(this Lesson lesson)
    {
        return new LessonResponse()
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            VideoUrl = lesson.VideoUrl,
            CourseId = lesson.CourseId,
        };
    }
    
    public static Package MapToPackage(this CreatePackageRequest request)
    {
        return new Package()
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
        };
    }
    
    public static PackageResponse MapToPackageResponse(this Package package)
    {
        return new PackageResponse()
        {
            Id = package.Id,
            Title = package.Title,
            Description = package.Description,
            Price = package.Price,
            CourseIds = package.Courses.Select(c => c.Id).ToList(),
        };
    }
}
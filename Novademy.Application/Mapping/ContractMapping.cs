using Novademy.Application.Models;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Requests.Course;
using Novademy.Contracts.Requests.Lesson;
using Novademy.Contracts.Requests.Package;
using Novademy.Contracts.Requests.Subscription;
using Novademy.Contracts.Responses.Auth;
using Novademy.Contracts.Responses.Course;
using Novademy.Contracts.Responses.Lesson;
using Novademy.Contracts.Responses.Package;
using Novademy.Contracts.Responses.Subscription;
using System;
using System.IO;
using System.Linq;

namespace Novademy.Application.Mapping;

public static class ContractMapping
{
    #region User
    public static User MapToUser(this RegisterRequest request)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Password = request.Password,
            Salt = Guid.NewGuid().ToString(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = "+994" + request.PhoneNumber,
            RoleId = request.RoleId,
            Group = request.Group,
            Sector = request.Sector
        };
    }
    #endregion

    #region Course
    public static Course MapToCourse(this CreateCourseRequest request)
    {
        return new Course
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Subject = request.Subject,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static CourseResponse MapToCourseResponse(this Course course)
    {
        return new CourseResponse
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Subject = course.Subject,
            ImageUrl = course.ImageUrl,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }
    #endregion

    #region Lesson
    public static Lesson MapToLesson(this CreateLessonRequest request)
    {
        return new Lesson
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Order = request.Order,
            Transcript = request.Transcript,
            IsFree = request.IsFree,
            CourseId = request.CourseId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static LessonResponse MapToLessonResponse(this Lesson lesson)
    {
        return new LessonResponse
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Description = lesson.Description,
            VideoUrl = lesson.VideoUrl,
            Order = lesson.Order,
            IsFree = lesson.IsFree,
            Transcript = lesson.Transcript,
            ImageUrl = lesson.ImageUrl,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            CourseId = lesson.CourseId
        };
    }
    #endregion

    #region Package
    public static Package MapToPackage(this CreatePackageRequest request)
    {
        return new Package
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static PackageResponse MapToPackageResponse(this Package package)
    {
        return new PackageResponse
        {
            Id = package.Id,
            Title = package.Title,
            Description = package.Description,
            Price = package.Price,
            ImageUrl = package.ImageUrl,
            CreatedAt = package.CreatedAt,
            UpdatedAt = package.UpdatedAt,
            CourseIds = package.Courses.Select(c => c.Id).ToList()
        };
    }
    #endregion

    #region Subscription
    public static Subscription MapToSubscription(this SubscriptionRequest request)
    {
        return new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            PackageId = request.PackageId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1)
        };
    }

    public static SubscriptionResponse MapToSubscriptionResponse(this Subscription subscription)
    {
        return new SubscriptionResponse
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            PackageId = subscription.PackageId,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            IsActive = subscription.IsActive
        };
    }
    #endregion
} 
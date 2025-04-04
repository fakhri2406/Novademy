using Novademy.Application.Models;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Requests.Course;
using Novademy.Contracts.Requests.Lesson;
using Novademy.Contracts.Requests.Package;
using Novademy.Contracts.Requests.Quiz;
using Novademy.Contracts.Requests.Subscription;
using Novademy.Contracts.Responses.Course;
using Novademy.Contracts.Responses.Lesson;
using Novademy.Contracts.Responses.Package;
using Novademy.Contracts.Responses.Subscription;

namespace Novademy.API.Mapping;

public static class ContractMapping
{
    #region User
    
    public static User MapToUser(this RegisterRequest request)
    {
        var phoneNumber = request.PhoneNumber;
        if (phoneNumber.StartsWith("0"))
        {
            phoneNumber = phoneNumber.Substring(1);
        }
        var formattedPhoneNumber = "+994" + phoneNumber;
        
        return new User
        {
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = formattedPhoneNumber,
            Password = request.Password,
            RoleId = request.RoleId,
            Group = request.Group,
            Sector = request.Sector,
            RegisteredAt = DateTime.UtcNow
        };
    }
    
    #endregion
    
    #region Course
    
    public static Course MapToCourse(this CreateCourseRequest request)
    {
        return new Course()
        {
            Title = request.Title,
            Description = request.Description,
            Subject = request.Subject,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public static CourseResponse MapToCourseResponse(this Course course)
    {
        return new CourseResponse()
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
        return new Lesson()
        {
            Title = request.Title,
            Description = request.Description,
            Order = request.Order,
            Transcript = request.Transcript,
            CourseId = request.CourseId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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
            Order = lesson.Order,
            Transcript = lesson.Transcript,
            ImageUrl = lesson.ImageUrl,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            CourseId = lesson.CourseId,
        };
    }
    
    #endregion
    
    #region Package
    
    public static Package MapToPackage(this CreatePackageRequest request)
    {
        return new Package()
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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
            ImageUrl = package.ImageUrl,
            CreatedAt = package.CreatedAt,
            UpdatedAt = package.UpdatedAt,
            CourseIds = package.Courses.Select(c => c.Id).ToList(),
        };
    }
    
    #endregion
    
    #region Subscription
    
    public static Subscription MapToSubscription(this SubscriptionRequest request)
    {
        return new Subscription()
        {
            UserId = request.UserId,
            PackageId = request.PackageId
        };
    }
    
    public static SubscriptionResponse MapToSubscriptionResponse(this Subscription subscription)
    {
        return new SubscriptionResponse()
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
    
    #region Quiz
    
    public static Quiz MapToQuiz(this CreateQuizRequest request)
    {
        return new Quiz
        {
            Title = request.Title,
            LessonId = request.LessonId
        };
    }
    
    public static Question MapToQuestion(this CreateQuestionRequest request)
    {
        return new Question
        {
            Text = request.Text,
            QuizId = request.QuizId
        };
    }

    public static Answer MapToAnswer(this CreateAnswerRequest request, Guid questionId)
    {
        return new Answer
        {
            Text = request.Text,
            IsCorrect = request.IsCorrect,
            QuestionId = questionId
        };
    }
    
    #endregion
}
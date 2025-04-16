using Dapper;
using Novademy.Application.Data.Dapper;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public SubscriptionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    #region Create
    
    public async Task<Subscription> CreateSubscriptionAsync(Subscription subscription)
    {
        const string sql = @"
            INSERT INTO Subscriptions (Id, UserId, PackageId, StartDate, EndDate, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Id, @UserId, @PackageId, @StartDate, @EndDate, @IsActive, @CreatedAt, @UpdatedAt)";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, subscription);
        
        return subscription;
    }
    
    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsByUserIdAsync(Guid userId)
    {
        const string sql = @"
            SELECT s.*, p.*, c.*
            FROM Subscriptions s
            LEFT JOIN Packages p ON s.PackageId = p.Id
            LEFT JOIN PackageCourses pc ON p.Id = pc.PackageId
            LEFT JOIN Courses c ON pc.CourseId = c.Id
            WHERE s.UserId = @UserId AND s.IsActive = 1";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var subscriptionDictionary = new Dictionary<Guid, Subscription>();
        var result = await connection.QueryAsync<Subscription, Package, Course, Subscription>(
            sql,
            (subscription, package, course) =>
            {
                if (!subscriptionDictionary.TryGetValue(subscription.Id, out var subscriptionEntry))
                {
                    subscriptionEntry = subscription;
                    subscriptionEntry.Package = package;
                    subscriptionEntry.Package.Courses = new List<Course>();
                    subscriptionDictionary.Add(subscriptionEntry.Id, subscriptionEntry);
                }
                
                if (course != null && !subscriptionEntry.Package.Courses.Any(c => c.Id == course.Id))
                {
                    subscriptionEntry.Package.Courses.Add(course);
                }
                
                return subscriptionEntry;
            },
            new { UserId = userId },
            splitOn: "Id,Id"
        );
        
        return subscriptionDictionary.Values;
    }
    
    #endregion
    
    #region Package Check
    
    public async Task<bool> HasActiveSubscriptionForPackageAsync(Guid userId, Guid packageId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Subscriptions
            WHERE UserId = @UserId AND PackageId = @PackageId AND IsActive = 1";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, PackageId = packageId });
        
        return count > 0;
    }
    
    #endregion
    
    #region Course Check
    
    public async Task<bool> HasActiveSubscriptionForCourseAsync(Guid userId, Guid courseId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Subscriptions s
            INNER JOIN PackageCourses pc ON s.PackageId = pc.PackageId
            WHERE s.UserId = @UserId AND pc.CourseId = @CourseId AND s.IsActive = 1";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, CourseId = courseId });
        
        return count > 0;
    }
    
    #endregion
    
    #region Lesson Check
    
    public async Task<bool> HasActiveSubscriptionForLessonAsync(Guid userId, Guid lessonId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM Subscriptions s
            INNER JOIN PackageCourses pc ON s.PackageId = pc.PackageId
            INNER JOIN Courses c ON pc.CourseId = c.Id
            INNER JOIN Lessons l ON c.Id = l.CourseId
            WHERE s.UserId = @UserId AND l.Id = @LessonId AND s.IsActive = 1";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, LessonId = lessonId });
        
        return count > 0;
    }
    
    #endregion
}
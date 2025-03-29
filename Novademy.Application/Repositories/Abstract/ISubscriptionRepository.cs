using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface ISubscriptionRepository
{
    Task<Subscription> CreateSubscriptionAsync(Subscription subscription);
    Task<IEnumerable<Subscription>> GetActiveSubscriptionsByUserIdAsync(Guid userId);
    Task<bool> HasActiveSubscriptionForPackageAsync(Guid userId, Guid packageId);
    Task<bool> HasActiveSubscriptionForCourseAsync(Guid userId, Guid courseId);
    Task<bool> HasActiveSubscriptionForLessonAsync(Guid userId, Guid lessonId);
}
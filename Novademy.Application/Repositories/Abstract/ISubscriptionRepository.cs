using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface ISubscriptionRepository
{
    Task<Subscription> CreateAsync(Subscription subscription);
    Task<IEnumerable<Subscription>> GetActiveByUserIdAsync(Guid userId);
    Task<bool> HasActiveForPackageAsync(Guid userId, Guid packageId);
    Task<bool> HasActiveForCourseAsync(Guid userId, Guid courseId);
    Task<bool> HasActiveForLessonAsync(Guid userId, Guid lessonId);
}
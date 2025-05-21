using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data.EFCore;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription> CreateSubscriptionAsync(Subscription subscription)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsByUserIdAsync(Guid userId)
    {
        var current = DateTime.UtcNow;
        var subscriptions = await _context.Subscriptions
            .Include(s => s.Package)
                .ThenInclude(p => p.Courses)
            .Where(s => s.UserId == userId && s.StartDate <= current && s.EndDate >= current)
            .ToListAsync();

        if (!subscriptions.Any())
        {
            throw new KeyNotFoundException("No active subscriptions found for the user.");
        }

        return subscriptions;
    }

    public async Task<bool> HasActiveSubscriptionForPackageAsync(Guid userId, Guid packageId)
    {
        var current = DateTime.UtcNow;
        return await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.PackageId == packageId && s.StartDate <= current && s.EndDate >= current);
    }

    public async Task<bool> HasActiveSubscriptionForCourseAsync(Guid userId, Guid courseId)
    {
        var current = DateTime.UtcNow;
        return await _context.Subscriptions
            .Include(s => s.Package)
                .ThenInclude(p => p.Courses)
            .Where(s => s.UserId == userId && s.StartDate <= current && s.EndDate >= current)
            .AnyAsync(s => s.Package.Courses.Any(c => c.Id == courseId));
    }

    public async Task<bool> HasActiveSubscriptionForLessonAsync(Guid userId, Guid lessonId)
    {
        var current = DateTime.UtcNow;
        return await _context.Subscriptions
            .Include(s => s.Package)
                .ThenInclude(p => p.Courses)
                    .ThenInclude(c => c.Lessons)
            .Where(s => s.UserId == userId && s.StartDate <= current && s.EndDate >= current)
            .AnyAsync(s => s.Package.Courses
                .SelectMany(c => c.Lessons)
                .Any(l => l.Id == lessonId));
    }
}
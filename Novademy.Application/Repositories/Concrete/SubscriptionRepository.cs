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
    
    #region Create

    public async Task<Subscription> CreateAsync(Subscription subscription)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }
    
    #endregion
    
    #region Read

    public async Task<IEnumerable<Subscription>> GetActiveByUserIdAsync(Guid userId)
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
    
    #endregion
    
    #region Check

    public async Task<bool> HasActiveForPackageAsync(Guid userId, Guid packageId)
    {
        var current = DateTime.UtcNow;
        return await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.PackageId == packageId && s.StartDate <= current && s.EndDate >= current);
    }

    public async Task<bool> HasActiveForCourseAsync(Guid userId, Guid courseId)
    {
        var current = DateTime.UtcNow;
        return await _context.Subscriptions
            .Include(s => s.Package)
                .ThenInclude(p => p.Courses)
            .Where(s => s.UserId == userId && s.StartDate <= current && s.EndDate >= current)
            .AnyAsync(s => s.Package.Courses.Any(c => c.Id == courseId));
    }

    public async Task<bool> HasActiveForLessonAsync(Guid userId, Guid lessonId)
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
    
    #endregion
}
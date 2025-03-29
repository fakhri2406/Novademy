using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data;
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
    
    public async Task<Subscription> CreateSubscriptionAsync(Subscription subscription)
    {
        subscription.Id = Guid.NewGuid();
        subscription.StartDate = DateTime.UtcNow;
        subscription.EndDate = DateTime.UtcNow.AddYears(1);
        
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        
        return subscription;
    }
    
    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsByUserIdAsync(Guid userId)
    {
        if (!_context.Users.Any(u => u.Id == userId))
        {
            throw new KeyNotFoundException("Invalid User ID.");
        }
        return await _context.Subscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .Include(s => s.Package)
            .ThenInclude(p => p!.Courses)
            .ToListAsync();
    }
    
    #endregion
    
    #region Package Check
    
    public async Task<bool> HasActiveSubscriptionForPackageAsync(Guid userId, Guid packageId)
    {
        if (!_context.Users.Any(u => u.Id == userId))
        {
            throw new KeyNotFoundException("Invalid User ID.");
        }
        if (!_context.Packages.Any(p => p.Id == packageId))
        {
            throw new KeyNotFoundException("Invalid Package ID.");
        }
        return await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.PackageId == packageId && s.IsActive);
    }
    
    #endregion
    
    #region Course Check
    
    public async Task<bool> HasActiveSubscriptionForCourseAsync(Guid userId, Guid courseId)
    {
        if (!_context.Users.Any(u => u.Id == userId))
        {
            throw new KeyNotFoundException("Invalid User ID.");
        }
        if (!_context.Courses.Any(c => c.Id == courseId))
        {
            throw new KeyNotFoundException("Invalid Course ID.");
        }
        return await _context.Subscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .AnyAsync(s => s.Package!.Courses.Any(c => c.Id == courseId));
    }
    
    #endregion
    
    #region Lesson Check
    
    public async Task<bool> HasActiveSubscriptionForLessonAsync(Guid userId, Guid lessonId)
    {
        if (!_context.Users.Any(u => u.Id == userId))
        {
            throw new KeyNotFoundException("Invalid User ID.");
        }
        if (!_context.Lessons.Any(l => l.Id == lessonId))
        {
            throw new KeyNotFoundException("Invalid Lesson ID.");
        }
        return await _context.Subscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .AnyAsync(s => s.Package!.Courses.Any(c => c.Lessons.Any(l => l.Id == lessonId)));
    }
    
    #endregion
}
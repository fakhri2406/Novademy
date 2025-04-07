using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data.EFCore;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class QuizRepository : IQuizRepository
{
    private readonly AppDbContext _context;
    
    public QuizRepository(AppDbContext context)
    {
        _context = context;
    }

    #region Create
    
    public async Task<Quiz> CreateQuizAsync(Quiz quiz)
    {
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();
        
        return quiz;
    }
    
    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Quiz>> GetQuizzesByLessonIdAsync(Guid lessonId)
    {
        if (!_context.Lessons.Any(l => l.Id == lessonId))
        {
            throw new KeyNotFoundException("Invalid Lesson ID.");
        }
        return await _context.Quizzes
            .Where(q => q.LessonId == lessonId)
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .ToListAsync();
    }
    
    public async Task<Quiz?> GetQuizByIdAsync(Guid id)
    {
        if (!_context.Quizzes.Any(q => q.Id == id))
        {
            throw new KeyNotFoundException("Invalid Quiz ID.");
        }
        return await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
    }
    
    #endregion
    
    #region Update
    
    public async Task<Quiz?> UpdateQuizAsync(Quiz quiz)
    {
        _context.Quizzes.Update(quiz);
        await _context.SaveChangesAsync();
        return quiz;
    }
    
    #endregion
    
    #region Delete
    
    public async Task DeleteQuizAsync(Guid id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        
        if (quiz is null)
        {
            throw new KeyNotFoundException("Invalid Quiz ID.");
        }
        
        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();
    }
    
    #endregion
}
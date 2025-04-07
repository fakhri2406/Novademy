using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data.EFCore;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class QuestionRepository : IQuestionRepository
{
    private readonly AppDbContext _context;
    
    public QuestionRepository(AppDbContext context)
    {
        _context = context;
    }
    
    #region Create
    
    public async Task<Question> CreateQuestionAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        
        return question;
    }
    
    #endregion
    
    #region Read
    
    public async Task<Question?> GetQuestionByIdAsync(Guid id)
    {
        if (!_context.Questions.Any(q => q.Id == id))
        {
            throw new KeyNotFoundException("Invalid Question ID.");
        }
        return await _context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
    }
    
    #endregion
}
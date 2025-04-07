using Novademy.Application.Data.EFCore;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class AnswerRepository : IAnswerRepository
{
    private readonly AppDbContext _context;
    
    public AnswerRepository(AppDbContext context)
    {
        _context = context;
    }

    #region Create
    
    public async Task<Answer> CreateAnswerAsync(Answer answer)
    {
        answer.Id = Guid.NewGuid();
        
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();
        
        return answer;
    }
    
    #endregion
}
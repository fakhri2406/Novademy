using Novademy.Application.Data;
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
    
    public async Task<Answer> CreateAnswerAsync(Answer answer)
    {
        answer.Id = Guid.NewGuid();
        
        _context.Answers.Add(answer);
        await _context.SaveChangesAsync();
        
        return answer;
    }
}
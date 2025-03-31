using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IQuestionRepository
{
    Task<Question> CreateQuestionAsync(Question question);
    Task<Question?> GetQuestionByIdAsync(Guid id);
}
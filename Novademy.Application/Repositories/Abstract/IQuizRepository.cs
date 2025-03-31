using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IQuizRepository
{
    Task<Quiz> CreateQuizAsync(Quiz quiz);
    Task<IEnumerable<Quiz>> GetQuizzesByLessonIdAsync(Guid lessonId);
    Task<Quiz?> GetQuizByIdAsync(Guid id);
    Task<Quiz?> UpdateQuizAsync(Quiz quiz);
    Task DeleteQuizAsync(Guid id);
}
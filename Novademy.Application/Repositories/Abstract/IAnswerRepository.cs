using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IAnswerRepository
{
    Task<Answer> CreateAnswerAsync(Answer answer);
}
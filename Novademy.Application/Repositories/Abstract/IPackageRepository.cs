using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface IPackageRepository
{
    Task<Package> CreateAsync(Package package, IFormFile? image);
    Task<IEnumerable<Package>> GetAllAsync();
    Task<Package?> GetByIdAsync(Guid id);
    Task<Package?> UpdateAsync(Package package, IFormFile? image);
    Task DeleteAsync(Guid id);
}
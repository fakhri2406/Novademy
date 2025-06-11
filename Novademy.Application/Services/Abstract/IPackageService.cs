using Novademy.Contracts.Requests.Package;
using Novademy.Contracts.Responses.Package;

namespace Novademy.Application.Services.Abstract;

public interface IPackageService
{
    Task<PackageResponse> CreateAsync(CreatePackageRequest request);
    Task<IEnumerable<PackageResponse>> GetAllAsync();
    Task<PackageResponse> GetByIdAsync(Guid id);
    Task<PackageResponse> UpdateAsync(Guid id, UpdatePackageRequest request);
    Task DeleteAsync(Guid id);
} 
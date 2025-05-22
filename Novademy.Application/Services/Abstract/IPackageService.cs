using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novademy.Contracts.Requests.Package;
using Novademy.Contracts.Responses.Package;

namespace Novademy.Application.Services.Abstract;

public interface IPackageService
{
    Task<IEnumerable<PackageResponse>> GetAllAsync();
    Task<PackageResponse> GetByIdAsync(Guid id);
    Task<PackageResponse> CreateAsync(CreatePackageRequest request);
    Task<PackageResponse> UpdateAsync(Guid id, UpdatePackageRequest request);
    Task DeleteAsync(Guid id);
} 
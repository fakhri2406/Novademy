using FluentValidation;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Mapping;
using Novademy.Application.Models;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Package;
using Novademy.Contracts.Responses.Package;

namespace Novademy.Application.Services.Concrete;

public class PackageService : IPackageService
{
    private readonly IPackageRepository _repo;
    private readonly ICourseRepository _courseRepo;
    private readonly IValidator<CreatePackageRequest> _createValidator;
    private readonly IValidator<UpdatePackageRequest> _updateValidator;

    public PackageService(
        IPackageRepository repo,
        ICourseRepository courseRepo,
        IValidator<CreatePackageRequest> createValidator,
        IValidator<UpdatePackageRequest> updateValidator)
    {
        _repo = repo;
        _courseRepo = courseRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    #region Create
    
    public async Task<PackageResponse> CreateAsync(CreatePackageRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        
        var package = request.MapToPackage();
        
        package.Courses = new List<Course>();
        foreach (var courseId in request.CourseIds)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course is not null)
                package.Courses.Add(course);
        }
        
        var created = await _repo.CreateAsync(package, request.Image);
        return created.MapToPackageResponse();
    }
    
    #endregion
    
    #region Read

    public async Task<IEnumerable<PackageResponse>> GetAllAsync()
    {
        var packages = await _repo.GetAllAsync();
        return packages.Select(p => p.MapToPackageResponse());
    }

    public async Task<PackageResponse> GetByIdAsync(Guid id)
    {
        var package = await _repo.GetByIdAsync(id);
        return package.MapToPackageResponse();
    }
    
    #endregion
    
    #region Update

    public async Task<PackageResponse> UpdateAsync(Guid id, UpdatePackageRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        var packageToUpdate = await _repo.GetByIdAsync(id);
        packageToUpdate.Title = request.Title;
        packageToUpdate.Description = request.Description;
        packageToUpdate.Price = request.Price;
        packageToUpdate.UpdatedAt = DateTime.UtcNow;
        packageToUpdate.Courses.Clear();
        foreach (var courseId in request.CourseIds)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course is not null)
                packageToUpdate.Courses.Add(course);
        }
        var updated = await _repo.UpdateAsync(packageToUpdate, request.Image);
        return updated.MapToPackageResponse();
    }
    
    #endregion
    
    #region Delete

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }
    
    #endregion
} 
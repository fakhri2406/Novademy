using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public async Task<IEnumerable<PackageResponse>> GetAllAsync()
    {
        var packages = await _repo.GetAllPackagesAsync();
        return packages.Select(p => p.MapToPackageResponse());
    }

    public async Task<PackageResponse> GetByIdAsync(Guid id)
    {
        var package = await _repo.GetPackageByIdAsync(id);
        return package.MapToPackageResponse();
    }

    public async Task<PackageResponse> CreateAsync(CreatePackageRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        
        var package = request.MapToPackage();
        
        package.Courses = new List<Course>();
        foreach (var courseId in request.CourseIds)
        {
            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course is not null)
                package.Courses.Add(course);
        }
        
        var created = await _repo.CreatePackageAsync(package, request.Image);
        return created.MapToPackageResponse();
    }

    public async Task<PackageResponse> UpdateAsync(Guid id, UpdatePackageRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        var packageToUpdate = await _repo.GetPackageByIdAsync(id);
        packageToUpdate.Title = request.Title;
        packageToUpdate.Description = request.Description;
        packageToUpdate.Price = request.Price;
        packageToUpdate.UpdatedAt = DateTime.UtcNow;
        packageToUpdate.Courses.Clear();
        foreach (var courseId in request.CourseIds)
        {
            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course is not null)
                packageToUpdate.Courses.Add(course);
        }
        var updated = await _repo.UpdatePackageAsync(packageToUpdate, request.Image);
        return updated.MapToPackageResponse();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeletePackageAsync(id);
    }
} 
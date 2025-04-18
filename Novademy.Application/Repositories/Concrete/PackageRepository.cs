using Dapper;
using Microsoft.AspNetCore.Http;
using Novademy.Application.Data.Dapper;
using Novademy.Application.ExternalServices.AzureBlobStorage;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class PackageRepository : IPackageRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IAzureBlobService _azureBlobService;
    
    public PackageRepository(IDbConnectionFactory connectionFactory, IAzureBlobService azureBlobService)
    {
        _connectionFactory = connectionFactory;
        _azureBlobService = azureBlobService;
    }
    
    #region Create

    public async Task<Package> CreatePackageAsync(Package package, IFormFile? image)
    {
        if (image is not null)
        {
            var imageUploadResult = await _azureBlobService.UploadFileAsync(image);
            package.ImageUrl = imageUploadResult;
        }
        
        const string sql = @"
            INSERT INTO Packages (Id, Title, Description, Price, ImageUrl, CreatedAt, UpdatedAt)
            VALUES (@Id, @Title, @Description, @Price, @ImageUrl, @CreatedAt, @UpdatedAt)";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, package);
        
        return package;
    }

    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Package>> GetAllPackagesAsync()
    {
        const string sql = @"
            SELECT p.*, c.*
            FROM Packages p
            LEFT JOIN PackageCourses pc ON p.Id = pc.PackageId
            LEFT JOIN Courses c ON pc.CourseId = c.Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var packageDictionary = new Dictionary<Guid, Package>();
        var result = await connection.QueryAsync<Package, Course, Package>(
            sql,
            (package, course) =>
            {
                if (!packageDictionary.TryGetValue(package.Id, out var packageEntry))
                {
                    packageEntry = package;
                    packageEntry.Courses = new List<Course>();
                    packageDictionary.Add(packageEntry.Id, packageEntry);
                }
                
                if (course != null && !packageEntry.Courses.Any(c => c.Id == course.Id))
                {
                    packageEntry.Courses.Add(course);
                }
                
                return packageEntry;
            },
            splitOn: "Id"
        );
        
        return packageDictionary.Values;
    }
    
    public async Task<Package?> GetPackageByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT p.*, c.*
            FROM Packages p
            LEFT JOIN PackageCourses pc ON p.Id = pc.PackageId
            LEFT JOIN Courses c ON pc.CourseId = c.Id
            WHERE p.Id = @Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var packageDictionary = new Dictionary<Guid, Package>();
        var result = await connection.QueryAsync<Package, Course, Package>(
            sql,
            (package, course) =>
            {
                if (!packageDictionary.TryGetValue(package.Id, out var packageEntry))
                {
                    packageEntry = package;
                    packageEntry.Courses = new List<Course>();
                    packageDictionary.Add(packageEntry.Id, packageEntry);
                }
                
                if (course != null && !packageEntry.Courses.Any(c => c.Id == course.Id))
                {
                    packageEntry.Courses.Add(course);
                }
                
                return packageEntry;
            },
            new { Id = id },
            splitOn: "Id"
        );
        
        var package = result.FirstOrDefault();
        if (package == null)
        {
            throw new KeyNotFoundException("Invalid Package ID.");
        }
        
        return package;
    }
    
    #endregion
    
    #region Update
    
    public async Task<Package?> UpdatePackageAsync(Package package, IFormFile? image)
    {
        if (image is not null)
        {
            var imageUploadResult = await _azureBlobService.UploadFileAsync(image);
            package.ImageUrl = imageUploadResult;
        }
        
        const string sql = @"
            UPDATE Packages 
            SET Title = @Title,
                Description = @Description,
                Price = @Price,
                ImageUrl = @ImageUrl,
                CourseIds = @CourseIds,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, package);
        
        return package;
    }
    
    #endregion
    
    #region Delete
    
    public async Task DeletePackageAsync(Guid id)
    {
        const string findImageSql = "SELECT ImageUrl FROM Packages WHERE Id = @Id";
        const string deletePackageSql = "DELETE FROM Packages WHERE Id = @Id";
    
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var imageUrl = await connection.QueryFirstOrDefaultAsync<string>(findImageSql, new { Id = id });
        
        if (!string.IsNullOrEmpty(imageUrl))
        {
            var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            await _azureBlobService.DeleteFileAsync(fileName);
        }
        
        var affectedRows = await connection.ExecuteAsync(deletePackageSql, new { Id = id });

        if (affectedRows == 0)
        {
            throw new KeyNotFoundException("Invalid Lesson ID.");
        }
    }
    
    #endregion
}
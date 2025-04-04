using System.Text;
using CloudinaryDotNet;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Novademy.Application.Data;
using Novademy.Application.Cloudinary;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Repositories.Concrete;
using Novademy.Application.Tokens;
using Novademy.Application.Validators.Auth;

namespace Novademy.Application.ServiceCollectionExtensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        
        return services;
    }
    
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Azure"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)));
        
        return services;
    }
    
    public static IServiceCollection AddTokens(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        
        var jwtSection = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        const int accessTokenLifeTime = 30;
        
        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = jwtSection["Issuer"]!;
            options.Audience = jwtSection["Audience"]!;
            options.AccessValidFor = TimeSpan.FromMinutes(accessTokenLifeTime);
            options.SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        });
        
        return services;
    }
    
    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        #region Validators
        
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        
        #endregion
        
        #region Cloudinary
        
        services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        
        services.AddSingleton<CloudinaryDotNet.Cloudinary>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
            return new CloudinaryDotNet.Cloudinary(new Account(config.CloudName, config.ApiKey, config.ApiSecret));
        });
        
        services.AddSingleton<IMediaUpload, MediaUpload>();
        
        #endregion
        
        return services;
    }
}
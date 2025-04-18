using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Novademy.Application.Data.Dapper;
using Novademy.Application.Data.EFCore;
using Novademy.Application.ExternalServices.AzureBlobStorage;
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
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IAnswerRepository, AnswerRepository>();
        
        return services;
    }
    
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptions => 
                {
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                    sqlServerOptions.MigrationsAssembly("Novademy.Application");
                }));
        
        return services;
    }
    
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        
        #region JWT Options
        
        var jwtSection = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        
        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = jwtSection["Issuer"]!;
            options.Audience = jwtSection["Audience"]!;
            options.AccessValidFor = TimeSpan.FromMinutes(30);
            options.SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        });
        
        #endregion
        
        #region Authentication Scheme
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };
            });
        
        #endregion
        
        return services;
    }
    
    public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        #region Validators
        
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        
        #endregion
        
        #region Azure Blob Storage
        
        services.Configure<AzureBlobOptions>(configuration.GetSection("AzureBlobStorage"));
        services.AddSingleton<IAzureBlobService, AzureBlobService>();
        
        #endregion
        
        return services;
    }
}
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Novademy.Application.Data;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Repositories.Concrete;
using Novademy.Application.Tokens;
using Novademy.Contracts.Validators.Auth;

namespace Novademy.Application.ServiceCollectionExtensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        #region DbContext
        
        services.AddDbContext<AppDbContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        
        #endregion
        
        #region Repositories
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        
        #endregion
        
        #region Validators
        
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        
        #endregion
        
        #region Tokens
        
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        
        #endregion
        
        return services;
    }
}
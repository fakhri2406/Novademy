using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Novademy.Application.Data.EFCore;
using Novademy.Application.ExternalServices.Cloudinary;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Repositories.Concrete;
using Novademy.Application.Tokens;
using Novademy.Application.Validators.Auth;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using Novademy.Application.ExternalServices.Email;
using Novademy.Application.ExternalServices.OpenAI;
using Novademy.Application.Services.Abstract;
using Novademy.Application.Services.Concrete;

namespace Novademy.Application.ServiceCollectionExtensions;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        
        return services;
    }
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        return services;
    }
    
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        #region Azure SQLServer DB
        
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
        
        #endregion
        
        #region Local DB
        
        // services.AddDbContext<AppDbContext>(options =>
        //     options.UseNpgsql(
        //         configuration.GetConnectionString("LocalConnection")));
        
        #endregion
        
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
        
        #region Cloudinary
        
        services.Configure<CloudinaryOptions>(configuration.GetSection("Cloudinary"));
        services.AddSingleton<Cloudinary>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CloudinaryOptions>>().Value;
            var account = new Account(options.CloudName, options.ApiKey, options.ApiSecret);
            return new Cloudinary(account);
        });
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        
        #endregion
        
        #region Email
        
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.AddSingleton<IEmailService, EmailService>();
        
        #endregion
        
        #region OpenAI
        
        services.Configure<OpenAIOptions>(configuration.GetSection("OpenAI"));
        services.AddHttpClient();
        services.AddSingleton<IOpenAIService, OpenAIService>();
        
        #endregion
        
        return services;
    }
}
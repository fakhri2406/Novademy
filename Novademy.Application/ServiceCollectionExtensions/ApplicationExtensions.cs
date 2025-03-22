using Microsoft.Extensions.DependencyInjection;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Repositories.Concrete;
using Novademy.Application.Tokens;

namespace Novademy.Application.ServiceCollectionExtensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        
        return services;
    }
}
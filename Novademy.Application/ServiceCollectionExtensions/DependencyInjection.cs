using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Novademy.Application.Data;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Repositories.Concrete;
using Novademy.Application.Tokens;

namespace Novademy.Application.ServiceCollectionExtensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        
        return services;
    }
}
using FCG02.UsersAPI.Application.Interfaces;
using FCG02.UsersAPI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FCG02.UsersAPI.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
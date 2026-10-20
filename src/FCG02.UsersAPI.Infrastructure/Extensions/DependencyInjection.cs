using FCG02.UsersAPI.Application.Interfaces;
using FCG02.UsersAPI.Domain.Interfaces;
using FCG02.UsersAPI.Infrastructure.Data;
using FCG02.UsersAPI.Infrastructure.Messaging;
using FCG02.UsersAPI.Infrastructure.Repositories;
using FCG02.UsersAPI.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG02.UsersAPI.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Configura o DbContext com a string de conexão do appsettings.json
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Registra o repositório de usuários
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddScoped<ITokenService, TokenService>();

        // Configuração do RabbitMQ
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}
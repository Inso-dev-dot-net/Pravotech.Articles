using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pravotech.Articles.Infrastructure.Persistence;

namespace Pravotech.Articles.Infrastructure;

/// <summary>
/// DI для PgSQL, чтобы подставить в WebAPI MS SQL в случае изменений
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует инфраструктурные сервисы, включая ArticlesDbContext
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфиг app</param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("ArticlesDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'ArticlesDatabase' is not configured");

        services.AddDbContext<ArticlesDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        return services;
    }
}
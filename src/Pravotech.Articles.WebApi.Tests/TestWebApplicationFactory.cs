using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pravotech.Articles.Infrastructure.Persistence;
using Pravotech.Articles.WebApi; // важно: чтобы видеть AssemblyMarker

namespace Pravotech.Articles.WebApi.Tests;

/// <summary>
/// Тестовая фабрика хоста, перенастраивающая ArticlesDbContext на InMemory
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<AssemblyMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? dbContextDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<ArticlesDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            string databaseName = $"ArticlesTests_{Guid.NewGuid()}";

            services.AddDbContext<ArticlesDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });

            using ServiceProvider sp = services.BuildServiceProvider();
            using IServiceScope scope = sp.CreateScope();
            ArticlesDbContext db = scope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

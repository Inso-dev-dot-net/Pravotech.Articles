using Microsoft.EntityFrameworkCore;
using Pravotech.Articles.Domain.Entities;

namespace Pravotech.Articles.Infrastructure.Persistence;

/// <summary>
/// DbContext для работы с каталогом статей и тегов
/// </summary>
public sealed class ArticlesDbContext : DbContext
{
    public ArticlesDbContext(DbContextOptions<ArticlesDbContext> options)
        : base(options)
    {
    }

    /// <summary>Набор статей</summary>
    public DbSet<Article> Articles => Set<Article>();

    /// <summary>Набор тегов</summary>
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArticlesDbContext).Assembly);
    }
}

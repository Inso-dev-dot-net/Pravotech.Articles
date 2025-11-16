using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pravotech.Articles.Domain.Entities;

namespace Pravotech.Articles.Infrastructure.Configurations;

/// <summary>
/// Конфигурация Article для EF Core
/// </summary>
internal sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.CreatedAtUtc)
            .IsRequired();

        builder.Property(a => a.UpdatedAtUtc)
            .IsRequired(false);

        // навигация через _tags
        builder.Navigation(a => a.Tags)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(a => a.Tags, tags =>
        {
            tags.ToTable("article_tags");

            tags.WithOwner()
                .HasForeignKey("ArticleId");

            tags.HasKey("ArticleId", "Position");

            tags.Property(t => t.TagId)
                .IsRequired();

            tags.Property(t => t.Position)
                .IsRequired();

            tags.HasIndex(t => t.TagId);
        });
    }
}

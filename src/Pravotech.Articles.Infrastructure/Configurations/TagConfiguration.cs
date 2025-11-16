using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pravotech.Articles.Domain.Entities;

namespace Pravotech.Articles.Infrastructure.Configurations;

/// <summary>
/// Конфигурация Tag для EF Core
/// </summary>
internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.NameNormalized)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(t => t.NameNormalized)
            .IsUnique();
    }
}

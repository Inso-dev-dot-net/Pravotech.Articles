using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pravotech.Articles.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixArticleTagsPrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        // Fluent API не хочет подтягивать конкретный Constraint, пост пятисотятся до создания Constraint ручками в PgAdmin
        migrationBuilder.Sql("""
            ALTER TABLE article_tags
            DROP CONSTRAINT IF EXISTS "PK_article_tags";
            ALTER TABLE article_tags
            ADD CONSTRAINT "PK_article_tags"
            PRIMARY KEY ("ArticleId", "Position");
        """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

    }
}

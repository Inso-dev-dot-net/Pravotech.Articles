namespace Pravotech.Articles.Domain.Entities;

public sealed class ArticleTag
{
    public Guid TagId { get; private set; }

    /// <summary>Позиция тега в списке статьи</summary>
    public int Position { get; private set; }

    private ArticleTag() { }

    /// <summary>Создает связь статья - тег</summary>
    /// <param name="tagId">Идентификатор тега</param>
    /// <param name="position">Позиция тега в списке статьи</param>
    /// <exception cref="ArgumentOutOfRangeException">Если position меньше 0</exception>
    public static ArticleTag Create(Guid tagId, int position)
    {

        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be non-negative");
        }

        ArticleTag tag = new()
        {
            TagId = tagId,
            Position = position
        };

        return tag;
    }

}

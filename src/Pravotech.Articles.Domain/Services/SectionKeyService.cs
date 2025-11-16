
using System.Security.Cryptography;
using System.Text;

namespace Pravotech.Articles.Domain.Services;

public sealed class SectionKeyService : ISectionKeyService
{
    private const int MaxSectionNameLength = 1024;
    /// <inheritdoc/>
    public string BuildSectionKey(IEnumerable<string> normalizedTagNames)
    {

        if(normalizedTagNames == null) throw new ArgumentNullException(nameof(normalizedTagNames));
        var names = normalizedTagNames
            .Distinct()
            .OrderBy(t => t);

        return string.Join("|", names);
    }

    /// <inheritdoc/>
    public Guid BuildSectionId(string sectionKey)
    {

        if (string.IsNullOrEmpty(sectionKey)) return Guid.Empty;

        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sectionKey)).Take(16).ToArray();

        Guid guid = new Guid(bytes);

        return guid;
    }

    /// <inheritdoc/>
    public string BuildSectionName(IEnumerable<string> tagNames)
    {
        if(tagNames == null) throw new ArgumentNullException(nameof(tagNames));

        var orderedTagNames = tagNames.OrderBy(t => t);
        var name = string.Join(", ", orderedTagNames);

        return name.Length <= MaxSectionNameLength ? name : name[..MaxSectionNameLength];
    }

}

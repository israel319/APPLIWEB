using AppPlusPlus.Domain.Entities.Catalogue;

namespace AppPlusPlus.Domain.Common;

/// <summary>
/// Types article pour le catalogue commandes en ligne (T_Art_Types.Description_Type).
/// </summary>
public static class ArticleTypeCatalog
{
    public const string OrderableDescription = "A commander";
    public const string NotOrderableDescription = "Non a commander";

    public static bool IsOrderableDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return false;

        return Normalize(description)
            .Equals(Normalize(OrderableDescription), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsOrderableArticle(Article article, IReadOnlyDictionary<int, string?> typesById) =>
        typesById.TryGetValue(article.IdType, out var desc) && IsOrderableDescription(desc);

    static string Normalize(string value) =>
        value.Trim().Replace('\u00A0', ' ');
}

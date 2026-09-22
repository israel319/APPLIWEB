namespace AppPlusPlus.Application.Common;

/// <summary>URLs publiques pour images catalogue — évite d'envoyer du base64 via SignalR.</summary>
public static class CatalogImageUrls
{
    public const string ArticlePrefix = "/catalog/img/a/";
    public const string CategoryPrefix = "/catalog/img/c/";

    public static string? Article(string idArticle, string? stored) =>
        string.IsNullOrWhiteSpace(stored) ? null : $"{ArticlePrefix}{idArticle}";

    public static string? Category(int idCategory, string? stored) =>
        string.IsNullOrWhiteSpace(stored) ? null : $"{CategoryPrefix}{idCategory}";
}

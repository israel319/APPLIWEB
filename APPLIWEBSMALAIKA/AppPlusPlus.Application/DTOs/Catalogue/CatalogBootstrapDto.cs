using AppPlusPlus.Domain.Entities.Catalogue;

namespace AppPlusPlus.Application.DTOs.Catalogue;

/// <summary>Données initiales du catalogue public (une passe BDD).</summary>
public class CatalogBootstrapDto
{
    public List<ArticleCategory> Categories { get; set; } = new();
    public HashSet<int> OrderableCategoryIds { get; set; } = new();
}

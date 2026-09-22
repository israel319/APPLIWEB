namespace AppPlusPlus.Application.Services.Stock;

/// <summary>Ligne de transfert inter-localisations (source → destination).</summary>
public record StockTransferLineRequest(
    string ArticleId,
    decimal Quantity,
    int SourceLocalisationId,
    int DestLocalisationId,
    int? CmdDetailId = null);

/// <summary>Sortie de stock (vente, livraison) à une localisation.</summary>
public record StockOutboundLineRequest(
    string ArticleId,
    decimal Quantity,
    int LocalisationId,
    int? IdDocumentDetail = null,
    decimal? PrixUnitaire = null);

/// <summary>Entrée de stock (réception inventaire, appro direct) à une localisation.</summary>
public record StockInboundLineRequest(
    string ArticleId,
    decimal Quantity,
    int LocalisationId,
    int? IdDocumentDetail = null,
    decimal? PrixUnitaire = null,
    DateOnly? DateExpiration = null);

/// <summary>Transformation inter-articles (source → destination).</summary>
public record StockTransformationRequest(
    string FromArticleId,
    string ToArticleId,
    decimal FromQuantity,
    decimal? ToQuantity,
    int FromLocalisationId,
    int? ToLocalisationId);

public class InternalTransferSaveRequest
{
    public int DestLocalisationId { get; set; }
    public int? SourceCmdId { get; set; }
    public string UserLogin { get; set; } = "";
    public string? Commentaire { get; set; }
    public string? Reference { get; set; }
    public List<StockTransferLineRequest> Lines { get; set; } = new();
}

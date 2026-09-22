using AppPlusPlus.Domain.Entities.Commandes;

namespace AppPlusPlus.Application.DTOs.Commandes;

/// <summary>
/// Complete detail view of a commande, including article rows
/// and livraison rows with payment/status information.
/// Used by the CommandeDetailView dialog.
/// </summary>
public class CommandeDetailViewDto
{
    public Commande Commande { get; set; } = null!;
    public List<CommandeArticleRow> ArticleRows { get; set; } = new();
    public List<CommandeLivraisonRow> LivraisonRows { get; set; } = new();
    public decimal DisplayTotalFc { get; set; }
    public decimal DisplayMontantPayeFc { get; set; }
    public decimal DisplayMontantRestFc { get; set; }
}

public class CommandeArticleRow
{
    public string ArticleName { get; set; } = "";
    public decimal Qte { get; set; }
    public decimal PU { get; set; }
    public decimal Montant { get; set; }
    public decimal QteLivree { get; set; }
}

public class CommandeLivraisonRow
{
    public int LivraisonId { get; set; }
    public string Porteur { get; set; } = "";
    public decimal Montant { get; set; }
    public DateTime? Date { get; set; }
    public int? FactureId { get; set; }
    public decimal TotalPaye { get; set; }
    public string StatusLabel { get; set; } = "";
    public string StatusStyle { get; set; } = "";
}

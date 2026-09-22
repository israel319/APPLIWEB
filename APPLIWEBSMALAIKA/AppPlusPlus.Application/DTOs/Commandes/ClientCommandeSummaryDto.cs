namespace AppPlusPlus.Application.DTOs.Commandes;

/// <summary>Vue synthétique d'une commande pour le portail client.</summary>
public class ClientCommandeSummaryDto
{
    public int CommandeId { get; set; }
    public DateTime? CreationDate { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; }
    public decimal MontantTotalFc { get; set; }
    public decimal MontantPaye { get; set; }
    public decimal MontantRest { get; set; }
    public decimal TotalQtyOrdered { get; set; }
    public decimal TotalQtyDelivered { get; set; }
    public int DeliveryProgressPercent { get; set; }
    public List<ClientCommandeLineDto> Lines { get; set; } = new();
    public List<ClientLivraisonSummaryDto> Livraisons { get; set; } = new();

    public string StatusLabel => Status switch
    {
        0 => "Nouvelle",
        1 => "En préparation",
        2 => "Livrée",
        3 => "Facturée",
        _ => "—"
    };

    public string StatusCssClass => Status switch
    {
        0 => "landing-cmd-status--new",
        1 => "landing-cmd-status--progress",
        2 => "landing-cmd-status--delivered",
        3 => "landing-cmd-status--paid",
        _ => "landing-cmd-status--neutral"
    };
}

public class ClientCommandeLineDto
{
    public string ArticleName { get; set; } = "";
    public decimal Qte { get; set; }
    public decimal QteLivree { get; set; }
    public decimal LineTotalFc { get; set; }
}

public class ClientLivraisonSummaryDto
{
    public int LivraisonId { get; set; }
    public DateTime? Date { get; set; }
    public string Porteur { get; set; } = "";
    public string StatusLabel { get; set; } = "";
}

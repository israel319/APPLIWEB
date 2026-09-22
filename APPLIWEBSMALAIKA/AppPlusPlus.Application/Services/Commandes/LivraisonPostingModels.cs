namespace AppPlusPlus.Application.Services.Commandes;

public class LivraisonLineInput
{
    public string ArticleId { get; set; } = "";
    public decimal Qte { get; set; }
    public decimal Pu { get; set; }
    public int? CommandDetailId { get; set; }
}

public class LivraisonSaveRequest
{
    public int? LivraisonId { get; set; }
    public string? Porteur { get; set; }
    public int? ClientId { get; set; }
    public int? CommandeId { get; set; }
    public DateTime Date { get; set; }
    public string UserLogin { get; set; } = "";
    public int LocalisationId { get; set; }
    public List<LivraisonLineInput> Lines { get; set; } = new();
}

using AppPlusPlus.Domain.Common;

namespace AppPlusPlus.Application.DTOs.Rapports;

public class BilanRowDto
{
    public int RowId { get; set; }
    public DateOnly Date { get; set; }
    public string Type { get; set; } = "";
    public string Reference { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Qte { get; set; }
    public decimal Montant { get; set; }
    public string Utilisateur { get; set; } = "";
}

public class BilanReportResult
{
    public List<BilanRowDto> Rows { get; set; } = new();
    public int NbFactures { get; set; }
    public decimal TotalVentes { get; set; }
    public decimal TotalAppros { get; set; }
    public decimal Benefice { get; set; }
    public decimal LatestTaux { get; set; }
}

public record VenteLigneReportDto(
    int NumFacture,
    string Client,
    string Article,
    double Qte,
    decimal? LineMontant,
    int? LineMoneyId,
    decimal? LineTaux,
    decimal? LineMontantApres,
    double LegacyPu,
    double LegacyLineTotal,
    decimal? FactMontant,
    int? FactMoneyId,
    decimal? FactMontantApres,
    decimal LegacyFactTotal,
    decimal Reduction,
    decimal FactTaux,
    DateOnly Date,
    string Utilisateur);

public class VentePeriodeGroupDto
{
    public string Label { get; set; } = "";
    public bool IsActive { get; set; }
    public bool IsHorsPeriode { get; set; }
    public int NbFactures { get; set; }
    public decimal Total { get; set; }
    public List<VenteLigneReportDto> Lignes { get; set; } = new();
}

public record StockReportRowDto(
    string IdArticle,
    string Description,
    double Price,
    decimal Qte,
    int Seuil,
    int QteMax,
    double ValeurStock,
    string Localisation);

public class CommandeArticleReportDto
{
    public string Name { get; set; } = "";
    public decimal QteOrder { get; set; }
    public decimal QteReceived { get; set; }
}

public class CommandeReportRowDto
{
    public int Id { get; set; }
    public string Fournisseur { get; set; } = "";
    public DateOnly? DateCommande { get; set; }
    public int Status { get; set; }
    public string User { get; set; } = "";
    public int NbArticles { get; set; }
    public decimal TotalOrder { get; set; }
    public decimal TotalReceived { get; set; }
    public List<CommandeArticleReportDto> Articles { get; set; } = new();
}

public class ApproMargeReportRowDto
{
    public int Id { get; set; }
    public string ArticleName { get; set; } = "";
    public decimal Qte { get; set; }
    public decimal PA { get; set; }
    public decimal PV { get; set; }
    public decimal Depense { get; set; }
    public decimal StockActuel { get; set; }
    public decimal Ben { get; set; }
    public decimal BenTotal { get; set; }
    public bool IsUsd { get; set; }
    public decimal Taux { get; set; }
    public DateOnly Date { get; set; }
    public string User { get; set; } = "";

    public decimal PA_Usd => CurrencyFormat.Split(PA, IsUsd, Taux).Usd;
    public decimal PA_Fc => CurrencyFormat.Split(PA, IsUsd, Taux).Fc;
    public decimal PV_Usd => CurrencyFormat.Split(PV, IsUsd, Taux).Usd;
    public decimal PV_Fc => CurrencyFormat.Split(PV, IsUsd, Taux).Fc;
    public decimal Depense_Usd => CurrencyFormat.Split(Depense, IsUsd, Taux).Usd;
    public decimal Depense_Fc => CurrencyFormat.Split(Depense, IsUsd, Taux).Fc;
    public decimal Ben_Usd => CurrencyFormat.Split(Ben, IsUsd, Taux).Usd;
    public decimal Ben_Fc => CurrencyFormat.Split(Ben, IsUsd, Taux).Fc;
    public decimal BenTotal_Usd => CurrencyFormat.Split(BenTotal, IsUsd, Taux).Usd;
    public decimal BenTotal_Fc => CurrencyFormat.Split(BenTotal, IsUsd, Taux).Fc;
}

public class ApproMargesReportResult
{
    public List<ApproMargeReportRowDto> Rows { get; set; } = new();
    public decimal LatestTaux { get; set; }
    public decimal TotalBeneficeUsd { get; set; }
}

public class FactMoisRowDto
{
    public int Id { get; set; }
    public string? DescriptionName { get; set; }
    public string? DescriptionArticle { get; set; }
    public decimal? Total { get; set; }
    public decimal? Reduction { get; set; }
    public decimal? TotalApresReduction { get; set; }
    public DateOnly Date { get; set; }
    public int Status { get; set; }
    public string? User { get; set; }
    public decimal Taux { get; set; }
}

public class FacturesMoisReportResult
{
    public List<FactMoisRowDto> Factures { get; set; } = new();
    public int TotalFactures { get; set; }
    public decimal TotalMontantUsd { get; set; }
    public decimal TotalReductionUsd { get; set; }
    public decimal TotalNetUsd { get; set; }
    public decimal LatestTaux { get; set; }
}

public class MouvementReportRowDto
{
    public int RowId { get; set; }
    public string IdArticle { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Qte { get; set; }
    public decimal Pu { get; set; }
    public DateOnly Date { get; set; }
    public string Mouvement { get; set; } = "";
    public string Utilisateur { get; set; } = "";
}

public class HistoriqueReportRowDto
{
    public int RowId { get; set; }
    public DateTime DateOp { get; set; }
    public DateOnly Date { get; set; }
    public string Type { get; set; } = "";
    public string Reference { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Montant { get; set; }
    public int StatusCode { get; set; }
    public string StatusLabel { get; set; } = "";
    public string Utilisateur { get; set; } = "";
    public int? FactId { get; set; }
}

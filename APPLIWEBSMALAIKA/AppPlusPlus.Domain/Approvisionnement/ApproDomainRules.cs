using AppPlusPlus.Domain.Entities.Approvisionnement;

namespace AppPlusPlus.Domain.Approvisionnement;

/// <summary>Agrégations approvisionnement — utilisées par StockHub, rapports et dashboard.</summary>
public static class ApproDomainRules
{
    public sealed record ApproLineMetrics(decimal Qte, decimal PA, decimal PV, decimal Ben, int LineCount);

    public static ApproLineMetrics Aggregate(Appro appro, IReadOnlyList<ApproDetail> details)
    {
        if (details.Count == 0)
        {
            return new ApproLineMetrics(
                appro.Qte,
                appro.PA ?? 0,
                appro.PV ?? 0,
                appro.BenTotal ?? ((appro.PV ?? 0) - (appro.PA ?? 0)) * appro.Qte,
                1);
        }

        var qte = details.Sum(d => d.Qte);
        var pa = qte > 0 ? details.Sum(d => (d.PA ?? 0) * d.Qte) / qte : 0;
        var pv = qte > 0 ? details.Sum(d => (d.PV ?? 0) * d.Qte) / qte : 0;
        var ben = details.Sum(d => d.BenTotal ?? ((d.PV ?? 0) - (d.PA ?? 0)) * d.Qte);
        return new ApproLineMetrics(qte, pa, pv, ben, details.Count);
    }

    public static string FormatArticleLabel(
        string primaryArticleId,
        string? primaryDescription,
        int lineCount)
    {
        var name = primaryDescription ?? primaryArticleId;
        if (lineCount <= 1) return name;
        return $"{name} (+{lineCount - 1} ligne{(lineCount > 2 ? "s" : "")})";
    }
}

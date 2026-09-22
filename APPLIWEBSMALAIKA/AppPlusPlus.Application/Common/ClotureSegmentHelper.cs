using AppPlusPlus.Domain.Entities.Finance;

namespace AppPlusPlus.Application.Common;

/// <summary>
/// Détermine si une facture appartient à une clôture approuvée ou à une nouvelle période après clôture.
/// </summary>
public static class ClotureSegmentHelper
{
    public static DateTime GetCutoff(Versement v) => v.DateTraitement ?? v.HeureCloture;

    public static DateTime? GetLastApprovedCutoff(DateOnly date, int localisationId, IEnumerable<Versement> versements)
    {
        var cutoffs = versements
            .Where(v => v.DateCloture == date
                && v.LocalisationId == localisationId
                && v.StatutCloture == 1)
            .Select(GetCutoff)
            .OrderByDescending(t => t)
            .ToList();

        return cutoffs.Count > 0 ? cutoffs[0] : null;
    }

    public static (bool EstCloturee, bool ApresCloture, bool ClotureEnAttente) ResolveForFact(
        DateOnly factDate,
        DateTime factDateSys,
        IEnumerable<int> factLocalisationIds,
        IEnumerable<int> userLocalisationIds,
        IEnumerable<Versement> versements)
    {
        var locs = factLocalisationIds.Any()
            ? factLocalisationIds.ToHashSet()
            : userLocalisationIds.ToHashSet();

        var relevant = versements
            .Where(v => v.DateCloture == factDate && locs.Contains(v.LocalisationId))
            .ToList();

        var clotureEnAttente = relevant.Any(v => v.StatutCloture == 0);

        var approved = relevant
            .Where(v => v.StatutCloture == 1)
            .OrderBy(GetCutoff)
            .ToList();

        if (approved.Count == 0)
            return (false, false, clotureEnAttente);

        var estCloturee = approved.Any(v => factDateSys <= GetCutoff(v));
        var lastCutoff = GetCutoff(approved[^1]);
        var apresCloture = !estCloturee && factDateSys > lastCutoff;

        return (estCloturee, apresCloture, clotureEnAttente);
    }
}

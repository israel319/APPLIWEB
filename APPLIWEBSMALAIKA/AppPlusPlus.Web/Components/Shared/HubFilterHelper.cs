namespace AppPlusPlus.Web.Components.Shared;

public static class HubFilterHelper
{
    public static bool MatchesSearch(string? search, params string?[] fields)
    {
        if (string.IsNullOrWhiteSpace(search)) return true;
        var s = search.Trim();
        return fields.Any(f => f != null && f.Contains(s, StringComparison.OrdinalIgnoreCase));
    }

    public static bool MatchesExact(string? filter, string? value) =>
        string.IsNullOrWhiteSpace(filter) || string.Equals(filter, value, StringComparison.OrdinalIgnoreCase);

    /// <summary>Filtre partiel (contient) — utilisé pour la saisie en direct dans les autocomplétions.</summary>
    public static bool MatchesFilter(string? filter, string? value) =>
        string.IsNullOrWhiteSpace(filter) ||
        (value != null && value.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase));

    public static List<string> DistinctNonEmpty(IEnumerable<string?> values) =>
        values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v).ToList();
}

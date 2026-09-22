namespace AppPlusPlus.Domain.Common;

/// <summary>Limites alignées sur les colonnes SQL / annotations EF.</summary>
public static class TextFieldLimits
{
    public const int ApproCommentaire = 50;
    public const int ApproReference = 50;
    public const int ApproUser = 50;
    public const int MouvementObservation = 255;
    public const int MouvementReference = 100;
    public const int MouvementUser = 50;

    public static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || maxLength <= 0)
            return value ?? string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    /// <summary>Ajoute un suffixe (ex. marqueur d'annulation) en préservant la fin si troncature nécessaire.</summary>
    public static string AppendWithLimit(string? current, string suffix, int maxLength)
    {
        current ??= string.Empty;
        if (string.IsNullOrEmpty(suffix))
            return Truncate(current, maxLength);

        var combined = string.IsNullOrWhiteSpace(current) ? suffix : $"{current} {suffix}";
        if (combined.Length <= maxLength)
            return combined;

        if (suffix.Length >= maxLength)
            return suffix[..maxLength];

        var prefixBudget = maxLength - suffix.Length - 1;
        if (prefixBudget <= 0)
            return suffix[..maxLength];

        var prefix = current.Length <= prefixBudget ? current : current[..prefixBudget];
        return $"{prefix} {suffix}";
    }
}

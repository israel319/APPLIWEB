using MudBlazor;

namespace AppPlusPlus.Web.Components.Shared;

/// <summary>Paramètres communs pour les listes de recherche MudAutocomplete.</summary>
public static class MudSearchDefaults
{
    public static DropdownSettings Dropdown { get; } = new()
    {
        Fixed = true,
        OverflowBehavior = OverflowBehavior.FlipNever
    };

    public const string PopoverClass = "search-autocomplete-popover";
    public const string InputClass = "search-autocomplete-input";
}

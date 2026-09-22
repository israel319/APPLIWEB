namespace AppPlusPlus.Application.Services.Parametres;

/// <summary>
/// Singleton service that caches global application settings (including the app name).
/// Loaded at startup, refreshable at any time.
/// </summary>
public interface IAppSettingsService
{
    const string KEY_APP_NAME = "AppName";
    const string KEY_LAYOUT_PREFERENCE = "LayoutPreference";
    const string KEY_THEME = "Theme";
    const string KEY_LANGUE = "Langue";
    const string LAYOUT_SIDEBAR = "Sidebar";
    const string LAYOUT_TOPBAR = "Topbar";

    string AppName { get; }
    string? Get(string key);
    Task LoadAsync();
    Task SetAsync(string key, string value);
    event Action? OnChange;
}

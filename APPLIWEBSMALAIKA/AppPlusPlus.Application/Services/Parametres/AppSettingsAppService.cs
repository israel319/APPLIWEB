using System.Collections.Concurrent;
using AppPlusPlus.Application.Interfaces.Repositories;

namespace AppPlusPlus.Application.Services.Parametres;

/// <summary>
/// Singleton service that caches global application settings (including the app name).
/// Loaded at startup, refreshable at any time.
/// Uses IParametresRepository to avoid direct DbContext dependency.
/// </summary>
public class AppSettingsAppService : IAppSettingsService
{
    private readonly IParametresRepository _parametresRepo;
    private ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public const string KEY_APP_NAME = "AppName";
    public const string KEY_LAYOUT_PREFERENCE = "LayoutPreference";
    public const string KEY_THEME = "Theme";
    public const string KEY_LANGUE = "Langue";
    public const string LAYOUT_SIDEBAR = "Sidebar";
    public const string LAYOUT_TOPBAR = "Topbar";
    private const string DEFAULT_APP_NAME = "App++";

    public event Action? OnChange;

    public AppSettingsAppService(IParametresRepository parametresRepo)
    {
        _parametresRepo = parametresRepo;
    }

    /// <inheritdoc />
    public string AppName => Get(KEY_APP_NAME) ?? DEFAULT_APP_NAME;

    /// <inheritdoc />
    public async Task LoadAsync()
    {
        try
        {
            var settings = await _parametresRepo.GetAllSettingsAsync();
            _cache = new ConcurrentDictionary<string, string>(
                settings.ToDictionary(
                    s => s.Key,
                    s => s.Value ?? "",
                    StringComparer.OrdinalIgnoreCase));
        }
        catch
        {
            _cache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <inheritdoc />
    public string? Get(string key)
        => _cache.TryGetValue(key, out var val) ? val : null;

    /// <inheritdoc />
    public async Task SetAsync(string key, string value)
    {
        await _parametresRepo.SetSettingAsync(key, value);
        _cache[key] = value;
        OnChange?.Invoke();
    }
}

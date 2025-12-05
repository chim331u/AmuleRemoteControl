using System.Globalization;
using System.Text.Json;
using AmuleRemoteControl.Components.Data.Setting;
using AmuleRemoteControl.Components.Interfaces;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service;

/// <summary>
/// Provides culture information for formatting and localization throughout the application.
/// Manages the current culture and persists user preferences.
/// </summary>
public class CultureProvider : ICultureProvider
{
    private readonly ILogger<CultureProvider> _logger;
    private readonly string _globalSettingsPath;
    private CultureInfo _currentCulture;
    private readonly object _cultureLock = new object();

    // Supported cultures for the application
    private static readonly string[] SupportedCultureNames = new[]
    {
        "en-US", // English (United States)
        "it-IT", // Italian (Italy)
        "de-DE", // German (Germany)
        "fr-FR", // French (France)
        "es-ES"  // Spanish (Spain)
    };

    public event EventHandler? CultureChanged;

    public CultureProvider(ILogger<CultureProvider> logger)
    {
        _logger = logger;
        _globalSettingsPath = Path.Combine(FileSystem.Current.AppDataDirectory, "GlobalSettings.json");

        // Load culture from settings or use default
        _currentCulture = LoadCultureFromSettings();

        // Apply culture to current thread
        ApplyCultureToCurrentThread();
    }

    /// <summary>
    /// Gets the current culture for the application.
    /// Thread-safe implementation.
    /// </summary>
    public CultureInfo GetCulture()
    {
        lock (_cultureLock)
        {
            return _currentCulture;
        }
    }

    /// <summary>
    /// Sets the application culture by culture name.
    /// Persists the setting and applies to current thread.
    /// </summary>
    public void SetCulture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            _logger.LogWarning("Attempted to set null or empty culture name");
            return;
        }

        try
        {
            var newCulture = CultureInfo.GetCultureInfo(cultureName);

            lock (_cultureLock)
            {
                if (_currentCulture.Name == newCulture.Name)
                {
                    _logger.LogDebug("Culture already set to {CultureName}", cultureName);
                    return;
                }

                _currentCulture = newCulture;
                _logger.LogInformation("Culture changed to {CultureName}", cultureName);
            }

            // Apply to current thread
            ApplyCultureToCurrentThread();

            // Persist to settings
            SaveCultureToSettings(cultureName);

            // Notify subscribers
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (CultureNotFoundException ex)
        {
            _logger.LogError(ex, "Invalid culture name: {CultureName}", cultureName);
            throw;
        }
    }

    /// <summary>
    /// Gets the list of supported culture names.
    /// </summary>
    public IReadOnlyList<string> GetSupportedCultures()
    {
        return SupportedCultureNames;
    }

    /// <summary>
    /// Gets the display name for a culture.
    /// </summary>
    public string GetCultureDisplayName(string cultureName)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            return $"{culture.NativeName} ({culture.EnglishName})";
        }
        catch (CultureNotFoundException)
        {
            _logger.LogWarning("Could not get display name for culture: {CultureName}", cultureName);
            return cultureName;
        }
    }

    /// <summary>
    /// Loads culture from GlobalSettings.json or returns default (en-US).
    /// </summary>
    private CultureInfo LoadCultureFromSettings()
    {
        try
        {
            if (File.Exists(_globalSettingsPath))
            {
                var json = File.ReadAllText(_globalSettingsPath);
                var wrapper = JsonSerializer.Deserialize<GlobalSettingWrapper>(json);
                var settings = wrapper?.Settings;
                var cultureSetting = settings?.FirstOrDefault(s => s.Key == "Culture");

                if (cultureSetting != null && !string.IsNullOrWhiteSpace(cultureSetting.Value))
                {
                    var culture = CultureInfo.GetCultureInfo(cultureSetting.Value);
                    _logger.LogInformation("Loaded culture from settings: {CultureName}", cultureSetting.Value);
                    return culture;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load culture from settings, using default");
        }

        // Default to English (United States)
        _logger.LogInformation("Using default culture: en-US");
        return CultureInfo.GetCultureInfo("en-US");
    }

    /// <summary>
    /// Saves the culture preference to GlobalSettings.json.
    /// </summary>
    private void SaveCultureToSettings(string cultureName)
    {
        try
        {
            List<GlobalSetting> settings;

            if (File.Exists(_globalSettingsPath))
            {
                var json = File.ReadAllText(_globalSettingsPath);
                var wrapper = JsonSerializer.Deserialize<GlobalSettingWrapper>(json);
                settings = wrapper?.Settings ?? new List<GlobalSetting>();
            }
            else
            {
                settings = new List<GlobalSetting>();
            }

            var cultureSetting = settings.FirstOrDefault(s => s.Key == "Culture");

            if (cultureSetting != null)
            {
                cultureSetting.Value = cultureName;
            }
            else
            {
                settings.Add(new GlobalSetting { Key = "Culture", Value = cultureName });
            }

            var newWrapper = new GlobalSettingWrapper { Settings = settings };
            var newJson = JsonSerializer.Serialize(newWrapper, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_globalSettingsPath, newJson);

            _logger.LogDebug("Saved culture to settings: {CultureName}", cultureName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save culture to settings");
        }
    }

    /// <summary>
    /// Applies the current culture to the current thread.
    /// This affects formatting, sorting, and other culture-specific operations.
    /// </summary>
    private void ApplyCultureToCurrentThread()
    {
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = _currentCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = _currentCulture;
            CultureInfo.CurrentCulture = _currentCulture;
            CultureInfo.CurrentUICulture = _currentCulture;

            _logger.LogDebug("Applied culture to current thread: {CultureName}", _currentCulture.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply culture to current thread");
        }
    }

    /// <summary>
    /// Wrapper class for GlobalSettings.json structure.
    /// </summary>
    private class GlobalSettingWrapper
    {
        public List<GlobalSetting> Settings { get; set; } = new();
    }
}

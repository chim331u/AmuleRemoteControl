using System.Globalization;

namespace AmuleRemoteControl.Components.Interfaces;

/// <summary>
/// Provides culture information for formatting and localization throughout the application.
/// </summary>
public interface ICultureProvider
{
    /// <summary>
    /// Gets the current culture for the application.
    /// </summary>
    /// <returns>The current CultureInfo instance.</returns>
    CultureInfo GetCulture();

    /// <summary>
    /// Sets the application culture by culture name (e.g., "en-US", "it-IT", "de-DE").
    /// </summary>
    /// <param name="cultureName">The culture name to set.</param>
    /// <exception cref="CultureNotFoundException">Thrown when the culture name is not valid.</exception>
    void SetCulture(string cultureName);

    /// <summary>
    /// Gets the available culture names supported by the application.
    /// </summary>
    /// <returns>List of supported culture names.</returns>
    IReadOnlyList<string> GetSupportedCultures();

    /// <summary>
    /// Gets the display name for a culture (e.g., "English (United States)" for "en-US").
    /// </summary>
    /// <param name="cultureName">The culture name.</param>
    /// <returns>The display name of the culture.</returns>
    string GetCultureDisplayName(string cultureName);

    /// <summary>
    /// Event raised when the culture changes.
    /// </summary>
    event EventHandler? CultureChanged;
}

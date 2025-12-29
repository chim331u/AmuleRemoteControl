using AmuleRemoteControl.Components.Data.AmuleModel;

namespace AmuleRemoteControl.Components.Interfaces
{
    /// <summary>
    /// Parser interface for aMule preferences page HTML.
    /// Extracts configuration settings from amuleweb-main-prefs.php HTML response.
    /// </summary>
    public interface IPreferencesParser
    {
        /// <summary>
        /// Parses HTML content from the preferences page and extracts all configuration settings.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-prefs.php</param>
        /// <returns>PreferenceModel with all parsed settings, or null if parsing fails</returns>
        /// <remarks>
        /// Currently uses a 200+ line switch statement in aMuleRemoteService.
        /// Sprint 9 (Task 3.9) will refactor this to use PreferenceMapping.json for maintainability.
        /// In Sprint 11 (Phase 4), this will be migrated to return Result&lt;PreferenceModel&gt;.
        /// </remarks>
        PreferenceModel? Parse(string html);
    }
}

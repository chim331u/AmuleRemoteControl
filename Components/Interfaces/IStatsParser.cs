using AmuleRemoteControl.Components.Data.AmuleModel;

namespace AmuleRemoteControl.Components.Interfaces
{
    /// <summary>
    /// Parser interface for aMule statistics page HTML.
    /// Extracts connection status from stats.php HTML response.
    /// </summary>
    public interface IStatsParser
    {
        /// <summary>
        /// Parses HTML content from the stats page and extracts connection statistics.
        /// </summary>
        /// <param name="html">Raw HTML from stats.php</param>
        /// <returns>Statistics object with Ed2k and Kad connection status, or null if parsing fails</returns>
        /// <remarks>
        /// Extracts Ed2k and Kad network status information.
        /// In Sprint 11 (Phase 4), this will be migrated to return Result&lt;Stats&gt;.
        /// </remarks>
        Stats? Parse(string html);
    }
}

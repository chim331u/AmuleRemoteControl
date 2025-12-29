using AmuleRemoteControl.Components.Data.AmuleModel;

namespace AmuleRemoteControl.Components.Interfaces
{
    /// <summary>
    /// Parser interface for aMule server page HTML.
    /// Extracts server list information from amuleweb-main-servers.php HTML response.
    /// </summary>
    public interface IServerParser
    {
        /// <summary>
        /// Parses HTML content from the server page and extracts server information.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-servers.php</param>
        /// <returns>List of servers with connection details, or null if parsing fails</returns>
        /// <remarks>
        /// Parses server name, description, address, user count, file count, and connection status.
        /// In Sprint 11 (Phase 4), this will be migrated to return Result&lt;List&lt;Servers&gt;&gt;.
        /// </remarks>
        List<Servers>? Parse(string html);
    }
}

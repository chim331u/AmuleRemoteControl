using AmuleRemoteControl.Components.Data.AmuleModel;

namespace AmuleRemoteControl.Components.Interfaces
{
    /// <summary>
    /// Parser interface for aMule download page HTML.
    /// Extracts download file information from amuleweb-main-dload.php HTML response.
    /// </summary>
    public interface IDownloadParser
    {
        /// <summary>
        /// Parses HTML content from the download page and extracts download file information.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-dload.php</param>
        /// <returns>List of download files, or null if parsing fails</returns>
        /// <remarks>
        /// In Sprint 11 (Phase 4), this will be migrated to return Result&lt;List&lt;DownloadFile&gt;&gt;
        /// for explicit error handling.
        /// </remarks>
        List<DownloadFile>? Parse(string html);
    }
}

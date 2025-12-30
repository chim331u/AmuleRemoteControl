using AmuleRemoteControl.Components.Data.AmuleModel;

namespace AmuleRemoteControl.Components.Interfaces
{
    /// <summary>
    /// Parser interface for aMule upload section HTML.
    /// Extracts upload file information from amuleweb-main-dload.php HTML response.
    /// </summary>
    public interface IUploadParser
    {
        /// <summary>
        /// Parses HTML content from the download page (upload section) and extracts upload file information.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-dload.php</param>
        /// <returns>List of upload files, or null if parsing fails</returns>
        /// <remarks>
        /// Upload data is embedded in the same page as downloads but in a different table (index 8).
        /// In Sprint 11 (Phase 4), this will be migrated to return Result&lt;List&lt;UploadFile&gt;&gt;.
        /// </remarks>
        List<UploadFile>? Parse(string html);
    }
}

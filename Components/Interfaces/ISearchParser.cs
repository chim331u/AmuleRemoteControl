using AmuleRemoteControl.Components.Data.AmuleModel;

namespace AmuleRemoteControl.Components.Interfaces
{
    /// <summary>
    /// Parser interface for aMule search results HTML.
    /// Extracts search results from amuleweb-main-search.php HTML response.
    /// </summary>
    public interface ISearchParser
    {
        /// <summary>
        /// Parses HTML content from the search page and extracts search results.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-search.php</param>
        /// <returns>List of search results with file names, sizes, and sources, or null if parsing fails</returns>
        /// <remarks>
        /// Parses file name, file size, source count, and search ID for each result.
        /// In Sprint 11 (Phase 4), this will be migrated to return Result&lt;List&lt;Search&gt;&gt;.
        /// Future enhancement: Could return SearchResult with pagination info.
        /// </remarks>
        List<Search>? Parse(string html);
    }
}

using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.AmuleModel;
using AmuleRemoteControl.Components.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service.Parsers
{
    /// <summary>
    /// Parser for aMule search results HTML.
    /// Extracts search results from amuleweb-main-search.php response.
    /// </summary>
    public class SearchParser : ISearchParser
    {
        private readonly ILogger<SearchParser> _logger;
        private readonly XPathConfiguration _xpathConfig;

        // Named constants for search result parsing state machine
        private const int SEARCH_ID_DETECTED = 1;
        private const int FILE_NAME_INDEX = 1;
        private const int FILE_SIZE_INDEX = 2;
        private const int SOURCES_INDEX = 3;

        /// <summary>
        /// Initializes a new instance of the SearchParser.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error tracking</param>
        /// <param name="xpathConfig">XPath configuration for version-aware HTML parsing</param>
        public SearchParser(ILogger<SearchParser> logger, XPathConfiguration xpathConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xpathConfig = xpathConfig ?? throw new ArgumentNullException(nameof(xpathConfig));
        }

        /// <summary>
        /// Parses HTML from the search page and extracts search results.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-search.php</param>
        /// <returns>List of search results with file names, sizes, and sources, or null if parsing fails</returns>
        public List<Search>? Parse(string html)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(html))
            {
                _logger.LogWarning("Parse: HTML input is null or empty");
                return null;
            }

            try
            {
                _logger.LogDebug("Parse: Starting search HTML parsing");

                var docServer = new HtmlDocument();
                docServer.LoadHtml(html);

                // Search results use different structure - select all table rows directly
                var rowNodes = docServer.DocumentNode.SelectNodes(_xpathConfig.SearchRowXPath);

                // Null safety: Check if rows exist
                if (rowNodes == null || rowNodes.Count == 0)
                {
                    _logger.LogInformation("Parse: No search result rows found (search may have returned no results)");
                    return new List<Search>(); // Return empty list instead of null
                }

                // Parse all cells from all rows
                var query = from row in rowNodes.Cast<HtmlNode>()
                            from cell in row.SelectNodes("th|td")?.Cast<HtmlNode>() ?? Enumerable.Empty<HtmlNode>()
                            select new { CellText = cell.InnerHtml };

                int columnCount = 0;
                Search currentSearch = new Search();
                var searchList = new List<Search>();

                // Iterate through cells using state machine pattern
                foreach (var cell in query)
                {
                    if (!string.IsNullOrEmpty(cell.CellText))
                    {
                        // Check if cell contains checkbox input (contains SearchId)
                        // Format: <input type="checkbox" name="hash_ABC123..." />
                        // Note: Must NOT contain "table" to avoid header checkboxes
                        if (cell.CellText.Contains("input type=\"checkbox\"") && !cell.CellText.Contains("table"))
                        {
                            currentSearch.SearchId = ExtractSearchId(cell.CellText);
                            columnCount = SEARCH_ID_DETECTED;
                            _logger.LogDebug($"Parse: Found search ID = {currentSearch.SearchId}");
                        }
                        else
                        {
                            // Map cell content to Search properties based on state
                            switch (columnCount)
                            {
                                case FILE_NAME_INDEX:
                                    currentSearch.FileName = cell.CellText;
                                    columnCount++;
                                    break;

                                case FILE_SIZE_INDEX:
                                    currentSearch.FileSize = cell.CellText;
                                    columnCount++;
                                    break;

                                case SOURCES_INDEX:
                                    currentSearch.Sources = cell.CellText;
                                    // End of row - add to list and reset
                                    searchList.Add(currentSearch);
                                    columnCount = 0;
                                    break;

                                default:
                                    // Unexpected state - reset
                                    columnCount = 0;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // Empty cell indicates end of row or data issue - reset
                        columnCount = 0;
                        currentSearch = new Search();
                    }
                }

                _logger.LogInformation($"Parse: Successfully retrieved {searchList.Count} search results");
                return searchList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parse: Error parsing search HTML");
                return null;
            }
        }

        /// <summary>
        /// Extracts search ID (file hash) from checkbox input HTML.
        /// Format: &lt;input type="checkbox" name="hash_ABC123DEF456..." /&gt; → "hash_ABC123DEF456..."
        /// </summary>
        /// <param name="input">HTML input element as string</param>
        /// <returns>Search ID from name attribute, or null if extraction fails</returns>
        private string? ExtractSearchId(string input)
        {
            try
            {
                var startIndex = input.IndexOf("name=\"");
                if (startIndex < 0)
                {
                    _logger.LogWarning($"ExtractSearchId: 'name=\"' not found in '{input}'");
                    return null;
                }

                var nameValueStart = startIndex + 6; // Skip 'name="'
                var endIndex = input.LastIndexOf("\"");

                if (endIndex <= nameValueStart)
                {
                    _logger.LogWarning($"ExtractSearchId: Invalid format in '{input}'");
                    return null;
                }

                var searchId = input.Substring(nameValueStart, endIndex - nameValueStart);
                return searchId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ExtractSearchId: Error extracting ID from '{input}'");
                return null;
            }
        }
    }
}

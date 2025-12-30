using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.AmuleModel;
using AmuleRemoteControl.Components.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service.Parsers
{
    /// <summary>
    /// Parser for aMule statistics page HTML.
    /// Extracts connection status from stats.php response.
    /// </summary>
    public class StatsParser : IStatsParser
    {
        private readonly ILogger<StatsParser> _logger;
        private readonly XPathConfiguration _xpathConfig;

        // Named constants for stat identifiers
        private const string ED2K_IDENTIFIER = "Ed2k";
        private const string KAD_IDENTIFIER = "Kad";
        private const string ED2K_PREFIX = "Ed2k :";
        private const string KAD_PREFIX = "Kad :";

        /// <summary>
        /// Initializes a new instance of the StatsParser.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error tracking</param>
        /// <param name="xpathConfig">XPath configuration for version-aware HTML parsing</param>
        public StatsParser(ILogger<StatsParser> logger, XPathConfiguration xpathConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xpathConfig = xpathConfig ?? throw new ArgumentNullException(nameof(xpathConfig));
        }

        /// <summary>
        /// Parses HTML from the stats page and extracts connection statistics.
        /// </summary>
        /// <param name="html">Raw HTML from stats.php</param>
        /// <returns>Statistics object with Ed2k and Kad connection status, or null if parsing fails</returns>
        public Stats? Parse(string html)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(html))
            {
                _logger.LogWarning("Parse: HTML input is null or empty");
                return null;
            }

            try
            {
                var docServer = new HtmlDocument();
                docServer.LoadHtml(html);

                // Select all tables using XPath configuration
                var tableNodes = docServer.DocumentNode.SelectNodes(_xpathConfig.StatsTableXPath);

                // Null safety: Check if tables exist
                if (tableNodes == null || tableNodes.Count == 0)
                {
                    _logger.LogWarning("Parse: No tables found in stats HTML");
                    return new Stats(); // Return empty stats instead of null
                }

                // Parse all table cells to find Ed2k and Kad status
                var query = from table in tableNodes.Cast<HtmlNode>()
                            from row in table.SelectNodes("tr")?.Cast<HtmlNode>() ?? Enumerable.Empty<HtmlNode>()
                            from cell in row.SelectNodes("td")?.Cast<HtmlNode>() ?? Enumerable.Empty<HtmlNode>()
                            select new { CellText = cell.InnerText };

                var stats = new Stats();
                bool foundEd2k = false;
                bool foundKad = false;

                // Iterate through all cells looking for Ed2k and Kad status
                foreach (var cell in query)
                {
                    if (string.IsNullOrWhiteSpace(cell.CellText))
                        continue;

                    // Check for Ed2k status
                    // Format: "Ed2k : Connected" or "Ed2k : Not Connected"
                    if (cell.CellText.Contains(ED2K_IDENTIFIER))
                    {
                        stats.Ed2kStat = ExtractStatus(cell.CellText, ED2K_PREFIX);
                        foundEd2k = true;
                        _logger.LogDebug($"Parse: Ed2k status = {stats.Ed2kStat}");
                    }

                    // Check for Kad status
                    // Format: "Kad : Firewalled" or "Kad : Connected" or "Kad : Not Connected"
                    if (cell.CellText.Contains(KAD_IDENTIFIER))
                    {
                        stats.KadStat = ExtractStatus(cell.CellText, KAD_PREFIX);
                        foundKad = true;
                        _logger.LogDebug($"Parse: Kad status = {stats.KadStat}");
                    }

                    // Early exit if both statuses found
                    if (foundEd2k && foundKad)
                    {
                        break;
                    }
                }

                // Log warning if expected stats not found
                if (!foundEd2k)
                {
                    _logger.LogWarning("Parse: Ed2k status not found in HTML");
                }

                if (!foundKad)
                {
                    _logger.LogWarning("Parse: Kad status not found in HTML");
                }

                _logger.LogInformation($"Parse: Successfully retrieved stats (Ed2k={stats.Ed2kStat ?? "null"}, Kad={stats.KadStat ?? "null"})");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parse: Error parsing stats HTML");
                return null;
            }
        }

        /// <summary>
        /// Extracts status text from cell content by removing prefix and cleaning whitespace.
        /// </summary>
        /// <param name="cellText">Cell text containing status (e.g., "Ed2k : Connected")</param>
        /// <param name="prefix">Prefix to remove (e.g., "Ed2k :")</param>
        /// <returns>Clean status text (e.g., "Connected")</returns>
        private string ExtractStatus(string cellText, string prefix)
        {
            try
            {
                // Remove newlines and the prefix, then trim whitespace
                var status = cellText
                    .Replace("\n", string.Empty)
                    .Replace(prefix, string.Empty)
                    .Trim();

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ExtractStatus: Error extracting status from '{cellText}'");
                return string.Empty;
            }
        }
    }
}

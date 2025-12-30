using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.AmuleModel;
using AmuleRemoteControl.Components.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AmuleRemoteControl.Components.Service.Parsers
{
    /// <summary>
    /// Parser for aMule download page HTML.
    /// Extracts download file information from amuleweb-main-dload.php response.
    /// </summary>
    public class DownloadParser : IDownloadParser
    {
        private readonly ILogger<DownloadParser> _logger;
        private readonly XPathConfiguration _xpathConfig;

        // Named constants for download table columns (replaces magic numbers in switch statement)
        private const int FILE_NAME_INDEX = 1;
        private const int FILE_SIZE_INDEX = 2;
        private const int COMPLETED_INDEX = 3;
        private const int DOWNLOAD_SPEED_INDEX = 4;
        private const int PROGRESS_BAR_INDEX = 5; // Skipped (contains HTML progress bar)
        private const int SOURCES_INDEX = 6;
        private const int STATUS_INDEX = 7;
        private const int PRIORITY_INDEX = 8;

        /// <summary>
        /// Initializes a new instance of the DownloadParser.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error tracking</param>
        /// <param name="xpathConfig">XPath configuration for version-aware HTML parsing</param>
        public DownloadParser(ILogger<DownloadParser> logger, XPathConfiguration xpathConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xpathConfig = xpathConfig ?? throw new ArgumentNullException(nameof(xpathConfig));
        }

        /// <summary>
        /// Parses HTML from the download page and extracts download file information.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-dload.php</param>
        /// <returns>List of download files, or null if parsing fails</returns>
        public List<DownloadFile>? Parse(string html)
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

                // Use XPathConfiguration instead of hardcoded Skip(6)
                var tableNodes = docServer.DocumentNode.SelectNodes(_xpathConfig.DownloadTableXPath);

                // Null safety: Check if tables exist
                if (tableNodes == null || tableNodes.Count <= _xpathConfig.DownloadTableIndex)
                {
                    _logger.LogWarning($"Parse: Could not find download table at index {_xpathConfig.DownloadTableIndex}");
                    return new List<DownloadFile>(); // Return empty list instead of null for better UX
                }

                // Select the download table using configured index
                var downloadTable = tableNodes.Skip(_xpathConfig.DownloadTableIndex).Take(1).FirstOrDefault();

                if (downloadTable == null)
                {
                    _logger.LogWarning("Parse: Download table is null after Skip/Take");
                    return new List<DownloadFile>();
                }

                // Select rows and skip header row(s) using configured skip count
                var rowNodes = downloadTable.SelectNodes("tr");

                if (rowNodes == null || rowNodes.Count <= _xpathConfig.DownloadRowSkipCount)
                {
                    _logger.LogInformation("Parse: No download rows found (table may be empty)");
                    return new List<DownloadFile>();
                }

                // Parse all cells from data rows
                var query = from row in rowNodes.Skip(_xpathConfig.DownloadRowSkipCount).Cast<HtmlNode>()
                            from cell in row.SelectNodes("th|td|input")?.Cast<HtmlNode>() ?? Enumerable.Empty<HtmlNode>()
                            select new { CellText = cell.InnerHtml };

                int columnCount = 0;
                DownloadFile currentFile = new DownloadFile();
                var fileList = new List<DownloadFile>();

                // Iterate through cells and build DownloadFile objects
                foreach (var cell in query)
                {
                    if (!string.IsNullOrEmpty(cell.CellText))
                    {
                        // Check if cell contains checkbox input (contains FileId)
                        if (cell.CellText.Contains("<input"))
                        {
                            currentFile.FileId = GetIdFromInput(cell.CellText);
                        }
                        else
                        {
                            columnCount++;

                            // Map cell content to DownloadFile properties based on column index
                            switch (columnCount)
                            {
                                case FILE_NAME_INDEX:
                                    currentFile.FileName = cell.CellText;
                                    break;

                                case FILE_SIZE_INDEX:
                                    currentFile.Size = cell.CellText;
                                    break;

                                case COMPLETED_INDEX:
                                    // Format: "350.2 MB (50.1%)" - contains both completed size and percentage
                                    currentFile.Completed = cell.CellText.Replace("&nbsp;", " ");
                                    if (!string.IsNullOrEmpty(currentFile.Completed))
                                    {
                                        currentFile.Progress = ConvertProgressToDouble(currentFile.Completed);
                                    }
                                    break;

                                case DOWNLOAD_SPEED_INDEX:
                                    currentFile.DownloadSpeed = cell.CellText;
                                    break;

                                case PROGRESS_BAR_INDEX:
                                    // Progress bar is HTML element, skip it (data is in COMPLETED_INDEX)
                                    break;

                                case SOURCES_INDEX:
                                    currentFile.Sources = cell.CellText;
                                    break;

                                case STATUS_INDEX:
                                    currentFile.Status = cell.CellText;
                                    break;

                                case PRIORITY_INDEX:
                                    currentFile.Priority = cell.CellText;
                                    // End of row - add to list
                                    fileList.Add(currentFile);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // Handle empty cells
                        if (columnCount == DOWNLOAD_SPEED_INDEX)
                        {
                            // Empty speed cell is valid (paused downloads have no speed)
                            columnCount++;
                        }
                        else
                        {
                            // Empty cell in other position indicates end of row - reset
                            columnCount = 0;
                            currentFile = new DownloadFile();
                        }
                    }
                }

                _logger.LogInformation($"Parse: Successfully retrieved {fileList.Count} downloads");
                return fileList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parse: Error parsing download HTML");
                return null;
            }
        }

        /// <summary>
        /// Converts progress string to double percentage.
        /// Format: "350.2 MB (50.1%)" → 50.1
        /// </summary>
        /// <param name="progress">Progress string from HTML</param>
        /// <returns>Percentage as double, or 0 if parsing fails</returns>
        private double ConvertProgressToDouble(string progress)
        {
            _logger.LogDebug($"ConvertProgressToDouble: Input = {progress}");

            var startIndex = progress.IndexOf("(");
            var percentIndex = progress.IndexOf("%");

            if (percentIndex >= 0 && startIndex >= 0)
            {
                try
                {
                    // Extract percentage value between '(' and '%'
                    var percentStr = progress.Substring(startIndex + 1, percentIndex - (startIndex + 1));

                    // Handle culture-specific decimal separators
                    // Italian culture uses "," as decimal separator, English uses "."
                    if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                    {
                        percentStr = percentStr.Replace(".", ",");
                    }
                    else
                    {
                        percentStr = percentStr.Replace(",", ".");
                    }

                    return Convert.ToDouble(percentStr);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"ConvertProgressToDouble: Error converting '{progress}'");
                    return 0;
                }
            }

            return 0;
        }

        /// <summary>
        /// Extracts file ID from checkbox input HTML.
        /// Format: &lt;input type="checkbox" name="file_12345" /&gt; → "file_12345"
        /// </summary>
        /// <param name="input">HTML input element as string</param>
        /// <returns>File ID from name attribute, or null if extraction fails</returns>
        private string? GetIdFromInput(string input)
        {
            try
            {
                var startIndex = input.IndexOf("name=\"") + 6;
                var endIndex = input.LastIndexOf("\"");

                if (startIndex < 6 || endIndex <= startIndex)
                {
                    _logger.LogWarning($"GetIdFromInput: Invalid input format: {input}");
                    return null;
                }

                return input.Substring(startIndex, endIndex - startIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetIdFromInput: Error extracting ID from '{input}'");
                return null;
            }
        }
    }
}

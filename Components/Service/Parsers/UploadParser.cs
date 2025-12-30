using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.AmuleModel;
using AmuleRemoteControl.Components.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service.Parsers
{
    /// <summary>
    /// Parser for aMule upload section HTML.
    /// Extracts upload file information from amuleweb-main-dload.php response.
    /// Upload data is embedded in the same page as downloads but in a different table.
    /// </summary>
    public class UploadParser : IUploadParser
    {
        private readonly ILogger<UploadParser> _logger;
        private readonly XPathConfiguration _xpathConfig;

        // Named constants for upload table columns (replaces magic numbers in switch statement)
        private const int FILE_NAME_INDEX = 1;
        private const int USER_NAME_INDEX = 2;
        private const int UPLOADED_INDEX = 3;
        private const int DOWNLOADED_INDEX = 4;
        private const int SKIP_COLUMN_5 = 5; // Skipped column (unknown/unused)
        private const int SKIP_COLUMN_6 = 6; // Skipped column (unknown/unused)
        private const int SPEED_INDEX = 7;

        /// <summary>
        /// Initializes a new instance of the UploadParser.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error tracking</param>
        /// <param name="xpathConfig">XPath configuration for version-aware HTML parsing</param>
        public UploadParser(ILogger<UploadParser> logger, XPathConfiguration xpathConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xpathConfig = xpathConfig ?? throw new ArgumentNullException(nameof(xpathConfig));
        }

        /// <summary>
        /// Parses HTML from the download page (upload section) and extracts upload file information.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-dload.php</param>
        /// <returns>List of upload files, or null if parsing fails</returns>
        public List<UploadFile>? Parse(string html)
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

                // Use XPathConfiguration instead of hardcoded Skip(8)
                var tableNodes = docServer.DocumentNode.SelectNodes(_xpathConfig.UploadTableXPath);

                // Null safety: Check if tables exist
                if (tableNodes == null || tableNodes.Count <= _xpathConfig.UploadTableIndex)
                {
                    _logger.LogWarning($"Parse: Could not find upload table at index {_xpathConfig.UploadTableIndex}");
                    return new List<UploadFile>(); // Return empty list instead of null for better UX
                }

                // Select the upload table using configured index
                var uploadTable = tableNodes.Skip(_xpathConfig.UploadTableIndex).Take(1).FirstOrDefault();

                if (uploadTable == null)
                {
                    _logger.LogWarning("Parse: Upload table is null after Skip/Take");
                    return new List<UploadFile>();
                }

                // Select rows and skip header row(s) using configured skip count
                var rowNodes = uploadTable.SelectNodes("tr");

                if (rowNodes == null || rowNodes.Count <= _xpathConfig.UploadRowSkipCount)
                {
                    _logger.LogInformation("Parse: No upload rows found (table may be empty)");
                    return new List<UploadFile>();
                }

                // Parse all cells from data rows
                var query = from row in rowNodes.Skip(_xpathConfig.UploadRowSkipCount).Cast<HtmlNode>()
                            from cell in row.SelectNodes("th|td|input")?.Cast<HtmlNode>() ?? Enumerable.Empty<HtmlNode>()
                            select new { CellText = cell.InnerHtml };

                int columnCount = 0;
                UploadFile currentFile = new UploadFile();
                var fileList = new List<UploadFile>();

                // Iterate through cells and build UploadFile objects
                foreach (var cell in query)
                {
                    // Check if cell has content (skip &nbsp; which aMule uses for empty cells)
                    if (!string.IsNullOrEmpty(cell.CellText) && !cell.CellText.Contains("&nbsp;"))
                    {
                        columnCount++;

                        // Map cell content to UploadFile properties based on column index
                        switch (columnCount)
                        {
                            case FILE_NAME_INDEX:
                                currentFile.FileName = cell.CellText;
                                break;

                            case USER_NAME_INDEX:
                                currentFile.UserName = cell.CellText;
                                break;

                            case UPLOADED_INDEX:
                                // Total data uploaded to this user
                                currentFile.Up = cell.CellText;
                                break;

                            case DOWNLOADED_INDEX:
                                // Total data user has downloaded
                                currentFile.Down = cell.CellText;
                                break;

                            case SPEED_INDEX:
                                // Current upload speed to this user
                                currentFile.Speed = cell.CellText;
                                // End of row - add to list and reset
                                fileList.Add(currentFile);
                                break;

                            default:
                                // Unknown column - reset to prevent misalignment
                                columnCount = 0;
                                break;
                        }
                    }
                    else
                    {
                        // Handle empty cells
                        if (columnCount == DOWNLOADED_INDEX || columnCount == SKIP_COLUMN_5)
                        {
                            // Empty cells in these positions are valid (increment to next column)
                            columnCount++;
                        }
                        else
                        {
                            // Empty cell in other position indicates end of row or data issue - reset
                            columnCount = 0;
                            currentFile = new UploadFile();
                        }
                    }
                }

                _logger.LogInformation($"Parse: Successfully retrieved {fileList.Count} uploads");
                return fileList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parse: Error parsing upload HTML");
                return null;
            }
        }
    }
}

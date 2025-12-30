using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.AmuleModel;
using AmuleRemoteControl.Components.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service.Parsers
{
    /// <summary>
    /// Parser for aMule server page HTML.
    /// Extracts server list information from amuleweb-main-servers.php response.
    /// </summary>
    public class ServerParser : IServerParser
    {
        private readonly ILogger<ServerParser> _logger;
        private readonly XPathConfiguration _xpathConfig;

        // Named constants for server table columns (replaces magic numbers in switch statement)
        private const int SERVER_NAME_INDEX = 1;
        private const int DESCRIPTION_INDEX = 2;
        private const int ADDRESS_INDEX = 3;
        private const int USERS_INDEX = 4;
        private const int FILES_INDEX = 5;

        /// <summary>
        /// Initializes a new instance of the ServerParser.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error tracking</param>
        /// <param name="xpathConfig">XPath configuration for version-aware HTML parsing</param>
        public ServerParser(ILogger<ServerParser> logger, XPathConfiguration xpathConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xpathConfig = xpathConfig ?? throw new ArgumentNullException(nameof(xpathConfig));
        }

        /// <summary>
        /// Parses HTML from the server page and extracts server information.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-servers.php</param>
        /// <returns>List of servers with connection details, or null if parsing fails</returns>
        public List<Servers>? Parse(string html)
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

                // Use XPathConfiguration instead of hardcoded Skip(1)
                var tableNodes = docServer.DocumentNode.SelectNodes(_xpathConfig.ServerTableXPath);

                // Null safety: Check if tables exist
                if (tableNodes == null || tableNodes.Count <= _xpathConfig.ServerTableIndex)
                {
                    _logger.LogWarning($"Parse: Could not find server table at index {_xpathConfig.ServerTableIndex}");
                    return new List<Servers>(); // Return empty list instead of null for better UX
                }

                // IMPORTANT: Match old behavior - process ALL tables after skipping ServerTableIndex
                // Old code: from table in SelectNodes("//table").Skip(1) <- processes ALL tables after first
                // This handles cases where server data might span multiple tables
                var query = from table in tableNodes.Skip(_xpathConfig.ServerTableIndex).Cast<HtmlNode>()
                            from row in table.SelectNodes("tr")?.Skip(_xpathConfig.ServerRowSkipCount).Cast<HtmlNode>() ?? Enumerable.Empty<HtmlNode>()
                            from cell in row.SelectNodes("th|td")?.Cast<HtmlNode>() ?? Enumerable.Empty<HtmlNode>()
                            select new { CellText = cell.InnerHtml };

                int columnCount = 0;
                Servers currentServer = new Servers();
                var serverList = new List<Servers>();

                // Iterate through cells and build Servers objects
                foreach (var cell in query)
                {
                    if (!string.IsNullOrEmpty(cell.CellText))
                    {
                        // Check if cell contains connection link (contains ServerId and Port)
                        if (cell.CellText.Contains("<a href"))
                        {
                            // Extract server ID and port from href attribute
                            // Format: <a href="amuleweb-main-servers.php?cmd=connect&ip=516650843&port=4321">Connect</a>
                            var serverIdAndPort = ExtractServerIdAndPort(cell.CellText);
                            if (serverIdAndPort.HasValue)
                            {
                                currentServer.ServerId = serverIdAndPort.Value.ServerId;
                                currentServer.Port = serverIdAndPort.Value.Port;
                            }
                        }
                        else
                        {
                            columnCount++;

                            // Map cell content to Servers properties based on column index
                            switch (columnCount)
                            {
                                case SERVER_NAME_INDEX:
                                    currentServer.ServerName = cell.CellText;
                                    break;

                                case DESCRIPTION_INDEX:
                                    currentServer.Description = cell.CellText;
                                    break;

                                case ADDRESS_INDEX:
                                    // Format: "192.168.1.100:4661" or IP:Port
                                    currentServer.Address = cell.CellText;
                                    break;

                                case USERS_INDEX:
                                    currentServer.Users = cell.CellText;
                                    break;

                                case FILES_INDEX:
                                    currentServer.Files = cell.CellText;
                                    // End of row - add to list and reset
                                    serverList.Add(currentServer);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // Empty cell indicates end of row - reset
                        columnCount = 0;
                        currentServer = new Servers();
                    }
                }

                _logger.LogInformation($"Parse: Successfully retrieved {serverList.Count} servers");
                return serverList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parse: Error parsing server HTML");
                return null;
            }
        }

        /// <summary>
        /// Extracts server ID and port from connection link HTML.
        /// Format: &lt;a href="amuleweb-main-servers.php?cmd=connect&ip=516650843&port=4321"&gt;Connect&lt;/a&gt;
        /// </summary>
        /// <param name="linkHtml">HTML containing the connection link</param>
        /// <returns>Tuple with ServerId and Port, or null if extraction fails</returns>
        private (string ServerId, string Port)? ExtractServerIdAndPort(string linkHtml)
        {
            try
            {
                // Extract server ID (ip parameter)
                var ipStartIndex = linkHtml.IndexOf("ip=");
                if (ipStartIndex < 0)
                {
                    _logger.LogWarning($"ExtractServerIdAndPort: 'ip=' parameter not found in '{linkHtml}'");
                    return null;
                }

                var ipValueStart = ipStartIndex + 3; // Skip "ip="
                var ipEndIndex = linkHtml.IndexOf("&port=", ipValueStart);
                if (ipEndIndex < 0)
                {
                    _logger.LogWarning($"ExtractServerIdAndPort: '&port=' parameter not found in '{linkHtml}'");
                    return null;
                }

                var serverId = linkHtml.Substring(ipValueStart, ipEndIndex - ipValueStart);

                // Extract port (port parameter)
                var portStartIndex = linkHtml.IndexOf("port=");
                if (portStartIndex < 0)
                {
                    _logger.LogWarning($"ExtractServerIdAndPort: 'port=' parameter not found in '{linkHtml}'");
                    return null;
                }

                var portValueStart = portStartIndex + 5; // Skip "port="
                var portEndIndex = linkHtml.IndexOf("\">");
                if (portEndIndex < 0 || portEndIndex <= portValueStart)
                {
                    _logger.LogWarning($"ExtractServerIdAndPort: Invalid port value format in '{linkHtml}'");
                    return null;
                }

                var port = linkHtml.Substring(portValueStart, portEndIndex - portValueStart);

                _logger.LogDebug($"ExtractServerIdAndPort: ServerId={serverId}, Port={port}");
                return (serverId, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ExtractServerIdAndPort: Error extracting from '{linkHtml}'");
                return null;
            }
        }
    }
}

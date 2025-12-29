using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Data
{
    /// <summary>
    /// Configuration for XPath selectors used in aMule HTML parsing.
    /// Supports version-specific XPath configurations to handle different aMule versions.
    /// </summary>
    public class XPathConfiguration
    {
        private readonly ILogger<XPathConfiguration>? _logger;
        private XPathConfig _config;

        // Named constants for table indices (previously hardcoded as magic numbers)

        /// <summary>
        /// Table index for download list (previously Skip(6))
        /// </summary>
        public int DownloadTableIndex => _config.DownloadTableIndex;

        /// <summary>
        /// Row skip count for download table header (previously Skip(1))
        /// </summary>
        public int DownloadRowSkipCount => _config.DownloadRowSkipCount;

        /// <summary>
        /// Table index for upload list (previously Skip(8))
        /// </summary>
        public int UploadTableIndex => _config.UploadTableIndex;

        /// <summary>
        /// Row skip count for upload table header (previously Skip(2))
        /// </summary>
        public int UploadRowSkipCount => _config.UploadRowSkipCount;

        /// <summary>
        /// Table index for server list (previously Skip(1))
        /// </summary>
        public int ServerTableIndex => _config.ServerTableIndex;

        /// <summary>
        /// Row skip count for server table header (previously Skip(3))
        /// </summary>
        public int ServerRowSkipCount => _config.ServerRowSkipCount;

        // XPath selectors for each page type

        /// <summary>
        /// XPath selector for download table: //table
        /// </summary>
        public string DownloadTableXPath => _config.DownloadTableXPath;

        /// <summary>
        /// XPath selector for upload table: //table
        /// </summary>
        public string UploadTableXPath => _config.UploadTableXPath;

        /// <summary>
        /// XPath selector for server table: //table
        /// </summary>
        public string ServerTableXPath => _config.ServerTableXPath;

        /// <summary>
        /// XPath selector for stats table: //table
        /// </summary>
        public string StatsTableXPath => _config.StatsTableXPath;

        /// <summary>
        /// XPath selector for search results: //tr
        /// </summary>
        public string SearchRowXPath => _config.SearchRowXPath;

        /// <summary>
        /// XPath selector for log content: //pre
        /// </summary>
        public string LogContentXPath => _config.LogContentXPath;

        /// <summary>
        /// XPath selector for preferences script: //script
        /// </summary>
        public string PreferencesScriptXPath => _config.PreferencesScriptXPath;

        /// <summary>
        /// Initializes XPathConfiguration with default values.
        /// In future sprints, this will load from xpaths.json configuration file.
        /// </summary>
        public XPathConfiguration(ILogger<XPathConfiguration>? logger = null)
        {
            _logger = logger;

            // Load default configuration
            // TODO (Sprint 7): Load from xpaths.json in Resources/Raw
            _config = GetDefaultConfiguration();

            _logger?.LogInformation("XPathConfiguration initialized with default values");
        }

        /// <summary>
        /// Gets the default XPath configuration for aMule 2.3.2+
        /// </summary>
        private XPathConfig GetDefaultConfiguration()
        {
            return new XPathConfig
            {
                Version = "default",
                DownloadTableIndex = 6,
                DownloadRowSkipCount = 1,
                UploadTableIndex = 8,
                UploadRowSkipCount = 2,
                ServerTableIndex = 1,
                ServerRowSkipCount = 3,
                DownloadTableXPath = "//table",
                UploadTableXPath = "//table",
                ServerTableXPath = "//table",
                StatsTableXPath = "//table",
                SearchRowXPath = "//tr",
                LogContentXPath = "//pre",
                PreferencesScriptXPath = "//script"
            };
        }

        /// <summary>
        /// Loads XPath configuration from JSON file for a specific aMule version.
        /// This will be implemented in Sprint 7 Task 3.1.
        /// </summary>
        /// <param name="version">aMule version (e.g., "2.3.2", "2.3.3")</param>
        public void LoadConfigurationForVersion(string version)
        {
            _logger?.LogInformation($"Loading XPath configuration for aMule version: {version}");

            // TODO (Sprint 7): Implement JSON loading
            // For now, use default configuration for all versions
            _config = GetDefaultConfiguration();

            _logger?.LogWarning($"Version-specific configuration not yet implemented. Using default configuration.");
        }

        /// <summary>
        /// Internal configuration model (will be loaded from JSON in Sprint 7)
        /// </summary>
        private class XPathConfig
        {
            public string Version { get; set; } = "default";

            // Table indices
            public int DownloadTableIndex { get; set; }
            public int DownloadRowSkipCount { get; set; }
            public int UploadTableIndex { get; set; }
            public int UploadRowSkipCount { get; set; }
            public int ServerTableIndex { get; set; }
            public int ServerRowSkipCount { get; set; }

            // XPath selectors
            public string DownloadTableXPath { get; set; } = "//table";
            public string UploadTableXPath { get; set; } = "//table";
            public string ServerTableXPath { get; set; } = "//table";
            public string StatsTableXPath { get; set; } = "//table";
            public string SearchRowXPath { get; set; } = "//tr";
            public string LogContentXPath { get; set; } = "//pre";
            public string PreferencesScriptXPath { get; set; } = "//script";
        }
    }
}

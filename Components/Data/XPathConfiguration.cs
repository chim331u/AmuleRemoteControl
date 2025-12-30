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
            // NOTE: Version-specific configurations can be loaded via LoadConfigurationForVersion()
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
        /// Falls back to "default" configuration if version-specific config not found.
        /// </summary>
        /// <param name="version">aMule version (e.g., "2.3.2", "2.3.3", "unknown")</param>
        public void LoadConfigurationForVersion(string version)
        {
            _logger?.LogInformation($"Loading XPath configuration for aMule version: {version}");

            // If version is unknown or empty, use default
            if (string.IsNullOrWhiteSpace(version) || version == "unknown")
            {
                _logger?.LogInformation("Using default configuration (unknown version)");
                _config = GetDefaultConfiguration();
                return;
            }

            try
            {
                // Try to load version-specific configuration from xpaths.json
                var versionConfig = LoadConfigurationFromJson(version);

                if (versionConfig != null)
                {
                    _config = versionConfig;
                    _logger?.LogInformation($"Loaded XPath configuration for aMule {version}");
                }
                else
                {
                    _logger?.LogWarning($"No specific configuration found for version {version}, using default");
                    _config = GetDefaultConfiguration();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error loading configuration for version {version}, using default");
                _config = GetDefaultConfiguration();
            }
        }

        /// <summary>
        /// Loads XPath configuration from xpaths.json for a specific version.
        /// </summary>
        /// <param name="version">aMule version</param>
        /// <returns>XPathConfig if found, null otherwise</returns>
        private XPathConfig? LoadConfigurationFromJson(string version)
        {
            try
            {
                // Try to load from Resources/Raw/xpaths.json
                var jsonPath = Path.Combine(FileSystem.AppDataDirectory, "..", "..", "Resources", "Raw", "xpaths.json");

                if (!File.Exists(jsonPath))
                {
                    _logger?.LogWarning($"xpaths.json not found at {jsonPath}");
                    return null;
                }

                var jsonContent = File.ReadAllText(jsonPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var document = JsonSerializer.Deserialize<XPathConfigDocument>(jsonContent, options);

                if (document?.Versions == null)
                {
                    _logger?.LogWarning("No versions found in xpaths.json");
                    return null;
                }

                // Try to get version-specific config, fall back to default
                if (document.Versions.TryGetValue(version, out var versionData))
                {
                    return ConvertToXPathConfig(version, versionData);
                }

                // If specific version not found, try "default"
                if (document.Versions.TryGetValue("default", out var defaultData))
                {
                    _logger?.LogInformation($"Version {version} not found in xpaths.json, using default");
                    return ConvertToXPathConfig("default", defaultData);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading xpaths.json");
                return null;
            }
        }

        /// <summary>
        /// Converts JSON version data to XPathConfig object.
        /// </summary>
        private XPathConfig ConvertToXPathConfig(string version, VersionData data)
        {
            return new XPathConfig
            {
                Version = version,
                DownloadTableIndex = data.DownloadTableIndex,
                DownloadRowSkipCount = data.DownloadRowSkipCount,
                UploadTableIndex = data.UploadTableIndex,
                UploadRowSkipCount = data.UploadRowSkipCount,
                ServerTableIndex = data.ServerTableIndex,
                ServerRowSkipCount = data.ServerRowSkipCount,
                DownloadTableXPath = data.DownloadTableXPath ?? "//table",
                UploadTableXPath = data.UploadTableXPath ?? "//table",
                ServerTableXPath = data.ServerTableXPath ?? "//table",
                StatsTableXPath = data.StatsTableXPath ?? "//table",
                SearchRowXPath = data.SearchRowXPath ?? "//tr",
                LogContentXPath = data.LogContentXPath ?? "//pre",
                PreferencesScriptXPath = data.PreferencesScriptXPath ?? "//script"
            };
        }

        /// <summary>
        /// Root document structure for xpaths.json
        /// </summary>
        private class XPathConfigDocument
        {
            public Dictionary<string, VersionData>? Versions { get; set; }
        }

        /// <summary>
        /// Version data from xpaths.json
        /// </summary>
        private class VersionData
        {
            public string? Description { get; set; }
            public int DownloadTableIndex { get; set; }
            public int DownloadRowSkipCount { get; set; }
            public int UploadTableIndex { get; set; }
            public int UploadRowSkipCount { get; set; }
            public int ServerTableIndex { get; set; }
            public int ServerRowSkipCount { get; set; }
            public string? DownloadTableXPath { get; set; }
            public string? UploadTableXPath { get; set; }
            public string? ServerTableXPath { get; set; }
            public string? StatsTableXPath { get; set; }
            public string? SearchRowXPath { get; set; }
            public string? LogContentXPath { get; set; }
            public string? PreferencesScriptXPath { get; set; }
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

using AmuleRemoteControl.Components.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AmuleRemoteControl.Components.Service
{
    /// <summary>
    /// Detects aMule version from HTML footer to enable version-specific parsing configuration.
    /// Uses regex pattern matching to extract version numbers from footer.php response.
    /// </summary>
    public class AmuleVersionDetector : IAmuleVersionDetector
    {
        private readonly ILogger<AmuleVersionDetector> _logger;

        // Regex pattern to match aMule version in footer HTML
        // Pattern: "aMule" followed by optional whitespace and version number (e.g., "2.3.2")
        private static readonly Regex VersionRegex = new Regex(
            @"aMule\s+([\d\.]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // Known supported versions - these have XPath configurations in xpaths.json
        private static readonly HashSet<string> SupportedVersions = new HashSet<string>
        {
            "2.3.2",
            "2.3.3"
        };

        // Version aliases - map alternative version strings to standard format
        private static readonly Dictionary<string, string> VersionAliases = new Dictionary<string, string>
        {
            { "2.3", "2.3.2" },      // Treat "2.3" as "2.3.2"
            { "2.3.0", "2.3.2" },    // Treat "2.3.0" as "2.3.2"
            { "2.3.1", "2.3.2" }     // Treat "2.3.1" as "2.3.2" (assume compatible)
        };

        /// <summary>
        /// Initializes a new instance of the AmuleVersionDetector.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and version detection tracking</param>
        public AmuleVersionDetector(ILogger<AmuleVersionDetector> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Detects the aMule version from HTML content (typically from footer.php).
        /// </summary>
        /// <param name="html">Raw HTML containing version information</param>
        /// <returns>Version string (e.g., "2.3.2", "2.3.3") or "unknown" if version cannot be detected</returns>
        /// <example>
        /// HTML: "&lt;div&gt;aMule 2.3.2&lt;/div&gt;" → Returns: "2.3.2"
        /// HTML: "&lt;div&gt;Unknown version&lt;/div&gt;" → Returns: "unknown"
        /// </example>
        public string DetectVersion(string html)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(html))
            {
                _logger.LogWarning("DetectVersion: HTML input is null or empty");
                return "unknown";
            }

            try
            {
                // Apply regex pattern to extract version
                var match = VersionRegex.Match(html);

                if (match.Success && match.Groups.Count > 1)
                {
                    var detectedVersion = match.Groups[1].Value.Trim();

                    // Check if this is an alias for a known version
                    if (VersionAliases.TryGetValue(detectedVersion, out var standardVersion))
                    {
                        _logger.LogInformation($"DetectVersion: Detected version '{detectedVersion}' mapped to '{standardVersion}'");
                        return standardVersion;
                    }

                    _logger.LogInformation($"DetectVersion: Successfully detected aMule version {detectedVersion}");
                    return detectedVersion;
                }

                // Version pattern not found in HTML
                _logger.LogWarning("DetectVersion: Could not find aMule version pattern in HTML");
                _logger.LogDebug($"DetectVersion: HTML content (first 200 chars): {html.Substring(0, Math.Min(200, html.Length))}");
                return "unknown";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DetectVersion: Error detecting version from HTML");
                return "unknown";
            }
        }

        /// <summary>
        /// Checks if a detected version is supported by the app.
        /// </summary>
        /// <param name="version">Version string to check</param>
        /// <returns>True if version is known and supported, false otherwise</returns>
        /// <remarks>
        /// Supported versions have dedicated XPath configurations in xpaths.json.
        /// Unsupported versions will fall back to "default" configuration.
        /// </remarks>
        public bool IsVersionSupported(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                _logger.LogWarning("IsVersionSupported: Version string is null or empty");
                return false;
            }

            // Check if version is in the supported list
            var isSupported = SupportedVersions.Contains(version);

            if (isSupported)
            {
                _logger.LogDebug($"IsVersionSupported: Version '{version}' is supported");
            }
            else
            {
                _logger.LogInformation($"IsVersionSupported: Version '{version}' is not explicitly supported (will use default configuration)");
            }

            return isSupported;
        }

        /// <summary>
        /// Gets all supported aMule versions.
        /// </summary>
        /// <returns>Collection of supported version strings</returns>
        public IEnumerable<string> GetSupportedVersions()
        {
            return SupportedVersions;
        }

        /// <summary>
        /// Gets a user-friendly message about version compatibility.
        /// </summary>
        /// <param name="version">Detected version</param>
        /// <returns>Compatibility message for display in UI</returns>
        public string GetCompatibilityMessage(string version)
        {
            if (string.IsNullOrWhiteSpace(version) || version == "unknown")
            {
                return "⚠️ aMule version could not be detected. Using default parsing configuration.";
            }

            if (IsVersionSupported(version))
            {
                return $"✅ aMule {version} is fully supported.";
            }

            // Check if it's a newer version (might be compatible)
            if (Version.TryParse(version, out var detectedVer))
            {
                var latestSupported = SupportedVersions
                    .Select(v => Version.TryParse(v, out var ver) ? ver : null)
                    .Where(v => v != null)
                    .OrderByDescending(v => v)
                    .FirstOrDefault();

                if (latestSupported != null && detectedVer > latestSupported)
                {
                    return $"⚠️ aMule {version} is newer than tested versions. Using default configuration. Please report compatibility issues.";
                }
            }

            return $"⚠️ aMule {version} is not officially supported. Using default configuration. Some features may not work correctly.";
        }
    }
}

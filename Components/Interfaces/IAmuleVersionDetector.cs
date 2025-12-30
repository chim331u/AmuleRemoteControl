namespace AmuleRemoteControl.Components.Interfaces
{
    /// <summary>
    /// Version detection interface for aMule web interface.
    /// Detects the aMule version from HTML footer to enable version-specific XPath configuration.
    /// </summary>
    public interface IAmuleVersionDetector
    {
        /// <summary>
        /// Detects the aMule version from HTML content (typically from footer.php).
        /// </summary>
        /// <param name="html">Raw HTML containing version information (usually footer.php response)</param>
        /// <returns>Version string (e.g., "2.3.2", "2.3.3") or "unknown" if version cannot be detected</returns>
        /// <remarks>
        /// Implementation in Sprint 10 (Task 3.11) will use regex pattern: aMule\s+([\d\.]+)
        /// This enables XPathConfiguration to select version-specific XPath selectors from xpaths.json.
        /// Falls back to "default" configuration if version is unknown.
        /// </remarks>
        /// <example>
        /// HTML: "&lt;div&gt;aMule 2.3.2&lt;/div&gt;" → Returns: "2.3.2"
        /// HTML: "&lt;div&gt;Unknown version&lt;/div&gt;" → Returns: "unknown"
        /// </example>
        string DetectVersion(string html);

        /// <summary>
        /// Checks if a detected version is supported by the app.
        /// </summary>
        /// <param name="version">Version string to check</param>
        /// <returns>True if version is known and supported, false otherwise</returns>
        /// <remarks>
        /// Used to show warnings in UI when connecting to unsupported aMule versions.
        /// Implementation will check against known versions in xpaths.json.
        /// </remarks>
        bool IsVersionSupported(string version);
    }
}

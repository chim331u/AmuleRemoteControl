using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.AmuleModel;
using AmuleRemoteControl.Components.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace AmuleRemoteControl.Components.Service.Parsers
{
    /// <summary>
    /// Parser for aMule preferences page HTML.
    /// Extracts configuration settings from amuleweb-main-prefs.php response.
    /// Uses JSON-driven mapping to eliminate the 200+ line switch statement.
    /// </summary>
    public class PreferencesParser : IPreferencesParser
    {
        private readonly ILogger<PreferencesParser> _logger;
        private readonly XPathConfiguration _xpathConfig;
        private readonly Dictionary<string, PreferenceMapping>? _mappings;

        /// <summary>
        /// Initializes a new instance of the PreferencesParser.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error tracking</param>
        /// <param name="xpathConfig">XPath configuration for version-aware HTML parsing</param>
        public PreferencesParser(ILogger<PreferencesParser> logger, XPathConfiguration xpathConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xpathConfig = xpathConfig ?? throw new ArgumentNullException(nameof(xpathConfig));

            // Load preference mappings from JSON
            _mappings = LoadPreferenceMappings();
        }

        /// <summary>
        /// Parses HTML from the preferences page and extracts all configuration settings.
        /// </summary>
        /// <param name="html">Raw HTML from amuleweb-main-prefs.php</param>
        /// <returns>PreferenceModel with all parsed settings, or null if parsing fails</returns>
        public PreferenceModel? Parse(string html)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(html))
            {
                _logger.LogWarning("Parse: HTML input is null or empty");
                return null;
            }

            if (_mappings == null || _mappings.Count == 0)
            {
                _logger.LogError("Parse: Preference mappings not loaded - cannot parse");
                return null;
            }

            try
            {
                var docServer = new HtmlDocument();
                docServer.LoadHtml(html);

                // Select script tags using XPath configuration
                var scriptNodes = docServer.DocumentNode.SelectNodes(_xpathConfig.PreferencesScriptXPath);

                // Null safety: Check if script tags exist
                if (scriptNodes == null || scriptNodes.Count == 0)
                {
                    _logger.LogWarning("Parse: No script tags found in preferences HTML");
                    return null;
                }

                var preferenceModel = new PreferenceModel();

                // Find the script tag containing preference initialization values
                foreach (var scriptNode in scriptNodes)
                {
                    var scriptContent = scriptNode.InnerHtml;

                    // Check if this script contains initvals (preference initialization)
                    if (!string.IsNullOrEmpty(scriptContent) && scriptContent.Contains("initvals"))
                    {
                        ParsePreferenceScript(scriptContent, preferenceModel);
                        break; // Only one script tag contains preferences
                    }
                }

                _logger.LogInformation("Parse: Successfully parsed preferences");
                return preferenceModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parse: Error parsing preferences HTML");
                return null;
            }
        }

        /// <summary>
        /// Parses JavaScript preference initialization script and populates PreferenceModel.
        /// Replaces the 200+ line switch statement with JSON-driven mapping.
        /// </summary>
        /// <param name="scriptContent">JavaScript content containing initvals array</param>
        /// <param name="preferenceModel">Model to populate with parsed values</param>
        private void ParsePreferenceScript(string scriptContent, PreferenceModel preferenceModel)
        {
            try
            {
                // Extract the preferences section from JavaScript
                // Format: initvals["key"] = "value"; initvals["key2"] = "value2"; ...
                int startIndex = scriptContent.IndexOf("initvals[");
                int endIndex = scriptContent.IndexOf("<!--");

                if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
                {
                    _logger.LogWarning("ParsePreferenceScript: Could not find initvals section in script");
                    return;
                }

                var preferencesSection = scriptContent.Substring(startIndex, endIndex - 2 - startIndex);
                var preferenceStatements = preferencesSection.TrimEnd().Split(';');

                _logger.LogDebug($"ParsePreferenceScript: Found {preferenceStatements.Length} preference statements");

                // Parse each preference statement
                foreach (var statement in preferenceStatements)
                {
                    if (statement.Length == 0)
                        continue;

                    // Extract key and value from: initvals["key"] = "value"
                    var keyValue = ParsePreferenceStatement(statement);
                    if (keyValue.HasValue)
                    {
                        SetPreferenceProperty(preferenceModel, keyValue.Value.Key, keyValue.Value.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ParsePreferenceScript: Error parsing preference script");
            }
        }

        /// <summary>
        /// Parses a single preference statement and extracts key-value pair.
        /// Format: initvals["max_up_limit"] = "100"
        /// </summary>
        /// <param name="statement">JavaScript statement</param>
        /// <returns>Key-value pair, or null if parsing fails</returns>
        private (string Key, string Value)? ParsePreferenceStatement(string statement)
        {
            try
            {
                // Extract key from: initvals["key"]
                int keyStart = 10; // Length of "initvals[\""
                int keyEnd = statement.IndexOf("]") - 1; // -1 for closing quote

                if (keyEnd <= keyStart)
                    return null;

                var key = statement.Substring(keyStart, keyEnd - keyStart);

                // Extract value from: = "value"
                int valueStart = statement.IndexOf("= ") + 3; // +3 for "= \""
                int valueEnd = statement.LastIndexOf("\"");

                if (valueEnd <= valueStart)
                    return null;

                var value = statement.Substring(valueStart, valueEnd - valueStart);

                return (key, value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"ParsePreferenceStatement: Error parsing '{statement}'");
                return null;
            }
        }

        /// <summary>
        /// Sets a property on PreferenceModel using reflection and JSON mapping.
        /// Replaces 200+ lines of switch statement with data-driven approach.
        /// </summary>
        /// <param name="model">PreferenceModel to update</param>
        /// <param name="key">Preference key from JavaScript (e.g., "max_up_limit")</param>
        /// <param name="value">Preference value from JavaScript (e.g., "100" or "1")</param>
        private void SetPreferenceProperty(PreferenceModel model, string key, string value)
        {
            // Look up mapping for this key
            if (!_mappings!.TryGetValue(key, out var mapping))
            {
                _logger.LogDebug($"SetPreferenceProperty: No mapping found for key '{key}' - skipping");
                return;
            }

            try
            {
                // Get the property using reflection
                var property = typeof(PreferenceModel).GetProperty(mapping.Property);

                if (property == null)
                {
                    _logger.LogWarning($"SetPreferenceProperty: Property '{mapping.Property}' not found on PreferenceModel");
                    return;
                }

                // Set value based on type
                if (mapping.Type == "bool")
                {
                    // Convert "1" to true, "0" to false
                    property.SetValue(model, value == "1");
                }
                else // string type
                {
                    property.SetValue(model, value);
                }

                _logger.LogDebug($"SetPreferenceProperty: Set {mapping.Property} = {value}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SetPreferenceProperty: Error setting property '{mapping.Property}' to '{value}'");
            }
        }

        /// <summary>
        /// Loads preference mappings from PreferenceMapping.json.
        /// This eliminates hardcoded switch statements and makes maintenance easier.
        /// </summary>
        /// <returns>Dictionary of preference mappings, or null if loading fails</returns>
        private Dictionary<string, PreferenceMapping>? LoadPreferenceMappings()
        {
            try
            {
                // Load JSON file from Resources/Raw
                var jsonPath = Path.Combine(FileSystem.AppDataDirectory, "..", "..", "Resources", "Raw", "PreferenceMapping.json");

                // For development, try loading from project directory
                if (!File.Exists(jsonPath))
                {
                    // Alternative path for embedded resources or different deployment
                    _logger.LogWarning($"LoadPreferenceMappings: PreferenceMapping.json not found at {jsonPath}");

                    // Try reading as embedded resource
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "AmuleRemoteControl.Resources.Raw.PreferenceMapping.json";

                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        var json = reader.ReadToEnd();
                        return ParseMappingJson(json);
                    }

                    _logger.LogError("LoadPreferenceMappings: Could not load PreferenceMapping.json from any location");
                    return null;
                }

                var jsonContent = File.ReadAllText(jsonPath);
                return ParseMappingJson(jsonContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoadPreferenceMappings: Error loading preference mappings");
                return null;
            }
        }

        /// <summary>
        /// Parses preference mapping JSON content.
        /// </summary>
        private Dictionary<string, PreferenceMapping>? ParseMappingJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var document = JsonSerializer.Deserialize<PreferenceMappingDocument>(json, options);
            return document?.Mappings;
        }

        /// <summary>
        /// Root document structure for PreferenceMapping.json
        /// </summary>
        private class PreferenceMappingDocument
        {
            public Dictionary<string, PreferenceMapping>? Mappings { get; set; }
        }

        /// <summary>
        /// Mapping configuration for a single preference
        /// </summary>
        private class PreferenceMapping
        {
            public string Property { get; set; } = string.Empty;
            public string Type { get; set; } = "string"; // "string" or "bool"
            public string? Description { get; set; }
        }
    }
}

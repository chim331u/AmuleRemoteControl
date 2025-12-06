using System.Text.RegularExpressions;
using System.Web;
using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.Ed2k;
using AmuleRemoteControl.Components.Interfaces;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service
{
    /// <summary>
    /// Parses and validates ed2k:// URLs used by the eDonkey2000/aMule network.
    /// Provides comprehensive validation and secure parsing with detailed error messages.
    /// </summary>
    public class Ed2kUrlParser : IEd2kUrlParser
    {
        private readonly ILogger<Ed2kUrlParser> _logger;

        // Regex pattern for ed2k:// URLs
        // Format: ed2k://|file|filename|filesize|filehash|optional_fields|/
        // - filename: Any characters (will be URL decoded)
        // - filesize: Positive integer (digits only)
        // - filehash: Exactly 32 hexadecimal characters (MD4 hash)
        private static readonly Regex Ed2kRegex = new Regex(
            @"^ed2k://\|file\|([^\|]+)\|(\d+)\|([A-F0-9]{32})\|(.*)/$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        // Minimum and maximum file sizes (1 byte to 16 TB)
        private const long MIN_FILE_SIZE = 1;
        private const long MAX_FILE_SIZE = 17592186044416L; // 16 TB in bytes

        public Ed2kUrlParser(ILogger<Ed2kUrlParser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Parses an ed2k:// URL and extracts all components with comprehensive validation.
        /// </summary>
        /// <param name="url">The ed2k:// URL to parse</param>
        /// <returns>Result containing the parsed Ed2kLink or error message</returns>
        public Result<Ed2kLink> Parse(string url)
        {
            // Step 1: Validate input is not null or empty
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("Ed2k parse failed: URL is null or empty");
                return Result<Ed2kLink>.Failure(GetErrorMessage(Ed2kParseError.NullOrEmpty));
            }

            // Step 2: Decode URL encoding if present
            // URLs shared from browsers may be percent-encoded
            string decodedUrl;
            try
            {
                decodedUrl = HttpUtility.UrlDecode(url);
                _logger.LogDebug($"Ed2k URL decoded: {decodedUrl[..Math.Min(50, decodedUrl.Length)]}...");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Ed2k URL decoding failed: {ex.Message}");
                return Result<Ed2kLink>.Failure(GetErrorMessage(Ed2kParseError.EncodingError), ex);
            }

            // Step 3: Apply regex pattern to extract components
            var match = Ed2kRegex.Match(decodedUrl);
            if (!match.Success)
            {
                _logger.LogWarning($"Ed2k parse failed: Invalid format for URL: {decodedUrl[..Math.Min(50, decodedUrl.Length)]}");
                return Result<Ed2kLink>.Failure(GetErrorMessage(Ed2kParseError.InvalidFormat));
            }

            // Step 4: Extract matched groups
            // Group 1: filename (URL encoded)
            // Group 2: filesize (as string)
            // Group 3: filehash (32 hex chars)
            // Group 4: optional fields (hashset, sources, etc.)
            string encodedFileName = match.Groups[1].Value;
            string fileSizeStr = match.Groups[2].Value;
            string fileHash = match.Groups[3].Value.ToUpperInvariant(); // Normalize to uppercase
            string optionalFields = match.Groups[4].Value;

            // Step 5: Decode filename
            // Filenames in ed2k links are often URL encoded
            string fileName;
            try
            {
                fileName = HttpUtility.UrlDecode(encodedFileName);

                // Validate filename is not empty after decoding
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    _logger.LogWarning("Ed2k parse failed: Filename is empty after decoding");
                    return Result<Ed2kLink>.Failure(GetErrorMessage(Ed2kParseError.MissingRequired));
                }

                _logger.LogInformation($"Ed2k parsing file: {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Ed2k filename decoding failed: {ex.Message}");
                return Result<Ed2kLink>.Failure(GetErrorMessage(Ed2kParseError.EncodingError), ex);
            }

            // Step 6: Parse and validate file size
            if (!long.TryParse(fileSizeStr, out long fileSize))
            {
                _logger.LogWarning($"Ed2k parse failed: Invalid file size format: {fileSizeStr}");
                return Result<Ed2kLink>.Failure(GetErrorMessage(Ed2kParseError.InvalidSize));
            }

            // Validate file size is within reasonable bounds
            if (fileSize < MIN_FILE_SIZE)
            {
                _logger.LogWarning($"Ed2k parse failed: File size too small: {fileSize} bytes");
                return Result<Ed2kLink>.Failure($"File size must be at least {MIN_FILE_SIZE} byte");
            }

            if (fileSize > MAX_FILE_SIZE)
            {
                _logger.LogWarning($"Ed2k parse failed: File size too large: {fileSize} bytes");
                return Result<Ed2kLink>.Failure($"File size exceeds maximum of 16 TB");
            }

            // Step 7: Validate file hash
            // Hash is already validated by regex (32 hex chars), but double-check
            if (fileHash.Length != 32)
            {
                _logger.LogWarning($"Ed2k parse failed: Invalid hash length: {fileHash.Length}");
                return Result<Ed2kLink>.Failure(GetErrorMessage(Ed2kParseError.InvalidHash));
            }

            // Verify all characters are hexadecimal (regex should have caught this, but be defensive)
            if (!fileHash.All(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')))
            {
                _logger.LogWarning($"Ed2k parse failed: Hash contains non-hex characters");
                return Result<Ed2kLink>.Failure(GetErrorMessage(Ed2kParseError.InvalidHash));
            }

            // Step 8: Parse optional fields (hashset, sources, etc.)
            // Format: h=hashset|p=partnerhash|s=server:port|s=server2:port2|
            string? hashSet = null;
            List<string>? sources = null;

            if (!string.IsNullOrEmpty(optionalFields))
            {
                var fields = optionalFields.Split('|', StringSplitOptions.RemoveEmptyEntries);
                foreach (var field in fields)
                {
                    if (field.StartsWith("h=", StringComparison.OrdinalIgnoreCase))
                    {
                        // AICH hashset (Advanced Intelligent Corruption Handling)
                        hashSet = field.Substring(2);
                        _logger.LogDebug($"Ed2k link has hashset: {hashSet[..Math.Min(8, hashSet.Length)]}...");
                    }
                    else if (field.StartsWith("s=", StringComparison.OrdinalIgnoreCase))
                    {
                        // Server source (ip:port or hostname:port)
                        var source = field.Substring(2);
                        if (!string.IsNullOrWhiteSpace(source))
                        {
                            sources ??= new List<string>();
                            sources.Add(source);
                            _logger.LogDebug($"Ed2k link has source: {source}");
                        }
                    }
                    // Other fields (p=partnerhash, etc.) can be added here in the future
                }
            }

            // Step 9: Create the Ed2kLink object
            var ed2kLink = new Ed2kLink
            {
                FileName = fileName,
                FileSize = fileSize,
                FileHash = fileHash,
                HashSet = hashSet,
                Sources = sources?.ToArray(),
                OriginalUrl = url // Store original URL for debugging
            };

            _logger.LogInformation($"Ed2k parse successful: {fileName} ({ed2kLink.FormattedFileSize})");

            return Result<Ed2kLink>.Success(ed2kLink);
        }

        /// <summary>
        /// Quickly validates whether a URL has a valid ed2k:// format.
        /// This is a lightweight check that doesn't perform full parsing.
        /// </summary>
        /// <param name="url">The URL to validate</param>
        /// <returns>true if the URL appears to be a valid ed2k:// link</returns>
        public bool IsValid(string url)
        {
            // Quick validation without full parsing
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            // Must start with ed2k:// (case insensitive)
            if (!url.StartsWith("ed2k://", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Must contain pipe separators (ed2k://|file|...)
            if (!url.Contains('|'))
            {
                return false;
            }

            // Must end with forward slash or pipe
            if (!url.EndsWith('/') && !url.EndsWith('|'))
            {
                return false;
            }

            // Count pipe separators - should have at least 4 for basic format
            // ed2k://|file|name|size|hash|/
            int pipeCount = url.Count(c => c == '|');
            if (pipeCount < 4)
            {
                return false;
            }

            // Passed all quick checks - likely valid
            // For comprehensive validation, use Parse()
            return true;
        }

        /// <summary>
        /// Gets a user-friendly error message for a specific parse error type.
        /// </summary>
        /// <param name="error">The error type</param>
        /// <returns>A detailed error message</returns>
        public string GetErrorMessage(Ed2kParseError error)
        {
            return error switch
            {
                Ed2kParseError.InvalidFormat =>
                    "Invalid ed2k link format. Expected format: ed2k://|file|filename|size|hash|/",

                Ed2kParseError.InvalidHash =>
                    "Invalid file hash. Hash must be exactly 32 hexadecimal characters (MD4).",

                Ed2kParseError.InvalidSize =>
                    "Invalid file size. Size must be a positive number.",

                Ed2kParseError.MissingRequired =>
                    "Missing required field. Ed2k links must have filename, size, and hash.",

                Ed2kParseError.NullOrEmpty =>
                    "URL is null or empty.",

                Ed2kParseError.EncodingError =>
                    "Failed to decode URL. The link may be corrupted or contain invalid characters.",

                Ed2kParseError.UnexpectedError =>
                    "An unexpected error occurred while parsing the ed2k link.",

                _ =>
                    "Unknown error occurred while parsing the ed2k link."
            };
        }
    }
}

using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Data.Ed2k;

namespace AmuleRemoteControl.Components.Interfaces
{
    /// <summary>
    /// Provides methods for parsing and validating ed2k:// URLs used by the eDonkey2000/aMule network.
    /// </summary>
    /// <remarks>
    /// Ed2k links follow this format:
    /// ed2k://|file|filename|filesize|filehash|/
    ///
    /// Where:
    /// - filename: UTF-8 encoded file name (may be URL encoded)
    /// - filesize: File size in bytes (positive integer)
    /// - filehash: 32-character hexadecimal MD4 hash
    ///
    /// Extended format with optional fields:
    /// ed2k://|file|filename|filesize|filehash|h=hashset|p=partnerhash|s=server:port|/
    ///
    /// Security considerations:
    /// - Always validate URLs before processing
    /// - Decode URL encoding carefully to prevent injection
    /// - Verify hash format to prevent malformed data
    /// - Check file size bounds to prevent overflow
    /// </remarks>
    public interface IEd2kUrlParser
    {
        /// <summary>
        /// Parses an ed2k:// URL and extracts its components.
        /// </summary>
        /// <param name="url">The ed2k:// URL to parse</param>
        /// <returns>
        /// A Result containing the parsed Ed2kLink on success,
        /// or an error message on failure
        /// </returns>
        /// <remarks>
        /// This method performs comprehensive validation:
        /// - URL format and structure
        /// - File hash format (32 hex characters)
        /// - File size (positive long)
        /// - URL encoding/decoding
        /// - Optional fields (hashset, sources)
        ///
        /// Example usage:
        /// <code>
        /// var result = parser.Parse("ed2k://|file|Ubuntu.iso|2877227008|5E0A6F1D...|/");
        /// if (result.IsSuccess)
        /// {
        ///     Console.WriteLine($"File: {result.Value.FileName}");
        ///     Console.WriteLine($"Size: {result.Value.FormattedFileSize}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine($"Parse error: {result.Error}");
        /// }
        /// </code>
        /// </remarks>
        Result<Ed2kLink> Parse(string url);

        /// <summary>
        /// Quickly validates whether a URL has a valid ed2k:// format
        /// without performing full parsing.
        /// </summary>
        /// <param name="url">The URL to validate</param>
        /// <returns>
        /// true if the URL appears to be a valid ed2k:// link;
        /// false otherwise
        /// </returns>
        /// <remarks>
        /// This is a lightweight check that verifies:
        /// - URL is not null or empty
        /// - URL starts with "ed2k://"
        /// - URL contains the required pipe separators
        /// - Basic structure looks valid
        ///
        /// For full validation and parsing, use Parse() instead.
        ///
        /// Example usage:
        /// <code>
        /// if (parser.IsValid(url))
        /// {
        ///     var result = parser.Parse(url);
        ///     // Process the parsed link
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Not a valid ed2k link");
        /// }
        /// </code>
        /// </remarks>
        bool IsValid(string url);

        /// <summary>
        /// Gets a detailed error message for a specific parse error type.
        /// </summary>
        /// <param name="error">The error type</param>
        /// <returns>A user-friendly error message</returns>
        /// <remarks>
        /// This method provides localized, detailed error messages
        /// that can be displayed to users.
        /// </remarks>
        string GetErrorMessage(Ed2kParseError error);
    }
}

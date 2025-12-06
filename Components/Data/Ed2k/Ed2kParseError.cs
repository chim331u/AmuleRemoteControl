namespace AmuleRemoteControl.Components.Data.Ed2k
{
    /// <summary>
    /// Specifies the type of error that occurred while parsing an ed2k:// URL.
    /// Used to provide specific error messages and handle different failure scenarios.
    /// </summary>
    public enum Ed2kParseError
    {
        /// <summary>
        /// The URL format is invalid or doesn't match the expected ed2k:// structure.
        /// Example: Missing pipes, wrong prefix, malformed structure.
        /// </summary>
        InvalidFormat,

        /// <summary>
        /// The file hash is invalid. Must be exactly 32 hexadecimal characters (MD4 hash).
        /// Example: Wrong length, non-hex characters, missing hash.
        /// </summary>
        InvalidHash,

        /// <summary>
        /// The file size is invalid. Must be a positive integer.
        /// Example: Negative number, zero, non-numeric value, too large.
        /// </summary>
        InvalidSize,

        /// <summary>
        /// A required field is missing from the URL.
        /// Example: Missing file name, missing size, missing hash.
        /// </summary>
        MissingRequired,

        /// <summary>
        /// The URL is null or empty.
        /// </summary>
        NullOrEmpty,

        /// <summary>
        /// URL encoding/decoding failed.
        /// Example: Invalid percent encoding, malformed UTF-8.
        /// </summary>
        EncodingError,

        /// <summary>
        /// An unexpected error occurred during parsing.
        /// Check the exception details for more information.
        /// </summary>
        UnexpectedError
    }
}

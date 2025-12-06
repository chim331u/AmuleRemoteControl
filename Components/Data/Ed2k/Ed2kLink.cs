namespace AmuleRemoteControl.Components.Data.Ed2k
{
    /// <summary>
    /// Represents a parsed ed2k:// link with all its components.
    /// Ed2k links are used by eDonkey2000/aMule networks to share files.
    /// </summary>
    /// <remarks>
    /// Standard format: ed2k://|file|filename|filesize|filehash|/
    /// Extended format: ed2k://|file|filename|filesize|filehash|h=hashset|p=partnerhash|s=serverip:port|/
    ///
    /// Example valid links:
    /// - ed2k://|file|Ubuntu-20.04.iso|2877227008|5E0A6F1D2C3B4A5D6E7F8A9B0C1D2E3F|/
    /// - ed2k://|file|Movie.avi|734003200|ABCD1234ABCD1234ABCD1234ABCD1234|h=ZYXW9876|/
    /// </remarks>
    public record Ed2kLink
    {
        /// <summary>
        /// Gets the name of the file (decoded from URL encoding).
        /// Example: "Ubuntu-20.04.iso" or "Movie (2023).avi"
        /// </summary>
        public string FileName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the size of the file in bytes.
        /// Example: 2877227008 (approximately 2.68 GB)
        /// </summary>
        public long FileSize { get; init; }

        /// <summary>
        /// Gets the MD4 hash of the file (32 hexadecimal characters, uppercase).
        /// This uniquely identifies the file on the ed2k network.
        /// Example: "5E0A6F1D2C3B4A5D6E7F8A9B0C1D2E3F"
        /// </summary>
        public string FileHash { get; init; } = string.Empty;

        /// <summary>
        /// Gets the optional AICH hashset identifier.
        /// AICH (Advanced Intelligent Corruption Handling) provides additional verification.
        /// Example: "ZYXW9876ZYXW9876ZYXW9876ZYXW9876"
        /// </summary>
        public string? HashSet { get; init; }

        /// <summary>
        /// Gets the optional array of source server addresses.
        /// Format: "ip:port" or "hostname:port"
        /// Example: ["192.168.1.100:4661", "server.example.com:4662"]
        /// </summary>
        public string[]? Sources { get; init; }

        /// <summary>
        /// Gets the original raw URL that was parsed.
        /// Useful for logging and debugging.
        /// </summary>
        public string OriginalUrl { get; init; } = string.Empty;

        /// <summary>
        /// Returns a human-readable description of the ed2k link.
        /// </summary>
        /// <returns>A formatted string with file details</returns>
        public override string ToString()
        {
            var sizeInMB = FileSize / (1024.0 * 1024.0);
            return $"Ed2k Link: {FileName} ({sizeInMB:F2} MB) - Hash: {FileHash[..8]}...";
        }

        /// <summary>
        /// Gets a formatted file size string (e.g., "2.68 GB", "734 MB", "1.5 KB").
        /// </summary>
        public string FormattedFileSize
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = FileSize;
                int order = 0;

                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }

                return $"{len:0.##} {sizes[order]}";
            }
        }

        /// <summary>
        /// Creates an Ed2kLink with required fields.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="fileSize">The file size in bytes</param>
        /// <param name="fileHash">The MD4 hash (32 hex characters)</param>
        /// <param name="originalUrl">The original URL</param>
        /// <returns>A new Ed2kLink instance</returns>
        public static Ed2kLink Create(string fileName, long fileSize, string fileHash, string originalUrl)
        {
            return new Ed2kLink
            {
                FileName = fileName,
                FileSize = fileSize,
                FileHash = fileHash,
                OriginalUrl = originalUrl
            };
        }
    }
}

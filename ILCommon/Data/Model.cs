using System;

using SQLite;

namespace ILCommon.Data.Model
{
    /// <summary>
    /// Represents a torrent file info, to skip duplicates, used by ArchiveTorrents
    /// </summary>
    public class MDownloadedTorr
    {
        [PrimaryKey, MaxLength (64)]
        public string HashId { get; set; }

        /// <summary>
        /// Torrent name extracted from metadata
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Length reported by torrent metadata
        /// </summary>
        public long Length { get; set; }

    }

    /// <summary>
    /// Represents an already downloaded file (not a torrent, but its actual content), used by ArchiveTorrents
    /// </summary>
    public class MDownloadedFile
    {
        [MaxLength (512), NotNull]
        public string FileName { get; set; }

        [NotNull]
        public long Length { get; set; }

    }

    /// <summary>
    /// Represents a log entry for a found hash, used by MetadataDownloader
    /// </summary>
    public class MTorrLog
    {
        [NotNull]
        public DateTime SeenAt { get; set; }

        [NotNull, MaxLength (64)]
        public string HashId { get; set; }

        public bool IsAnnounce { get; set; }
    }

    /// <summary>
    /// Represents a torrent metadata, used by MetadataDownloader
    /// </summary>
    public class MTorr
    {
        [PrimaryKey, MaxLength (64)]
        public string HashId { get; set; }

        public string Name { get; set; }

        public string Comment { get; set; }

        public int CountSeen { get; set; }

        public DateTime LastSeen { get; set; }

        public long Length { get; set; }

        public bool Processed { get; set; }

        public bool Timeout { get; set; }

        public bool Downloaded { get; set; }

        public bool IsAnnounce { get; set; }

        public DateTime DownloadedTime { get; set; }

        public DateTime ProcessedTime { get; set; }

    }

}

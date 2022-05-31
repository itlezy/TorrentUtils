using System;

using SQLite;

namespace ILCommon.Data.Model
{
    /// <summary>
    /// Represents a torrent file info, to skip duplicates, used by ArchiveTorrents
    /// </summary>
    public class MDownloadedTorr
    {
        [PrimaryKey, MaxLength (256)]
        public string HashId { get; set; }

        public string Name { get; set; }

        public long Length { get; set; }

    }

    /// <summary>
    /// Represents an already downloaded file (not a torrent, but its actual content), used by ArchiveTorrents
    /// </summary>
    public class MDownloadedFile
    {
        [PrimaryKey, MaxLength (512)]
        public string FileName { get; set; }

        public long Length { get; set; }

    }

    /// <summary>
    /// Represents a log entry for a found hash, used by MetadataDownloader
    /// </summary>
    public class MTorrLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public DateTime SeenAt { get; set; }
        [Indexed]
        public string HashId { get; set; }
    }

    /// <summary>
    /// Represents a torrent metadata, used by MetadataDownloader
    /// </summary>
    public class MTorr
    {
        [PrimaryKey, MaxLength (40)]
        public string HashId { get; set; }

        public string Name { get; set; }
        public string Comment { get; set; }

        [Indexed]
        public int CountSeen { get; set; }
        public long Length { get; set; }

        public bool Processed { get; set; }
        public bool Timeout { get; set; }
        public bool Downloaded { get; set; }

        public DateTime DownloadedTime { get; set; }
        public DateTime LastSeen { get; set; }

        public DateTime ProcessedTime { get; set; }

    }

}

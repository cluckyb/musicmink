using MusicMinkAppLayer.Enums;
using SQLite;

namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Defines the structure of the Song table for the SQLite database 
    /// </summary>
    [Table("SongTable")]
    public class SongTable : BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int SongId { get; set; }

        public static class Properties
        {
            public const string SongId = "SongId";

            public const string AlbumId = "AlbumId";
            public const string ArtistId = "ArtistId";
            public const string Duration = "Duration";
            public const string LastPlayed = "LastPlayed";
            public const string Name = "Name";
            public const string Origin = "Origin";
            public const string PlayCount = "PlayCount";
            public const string Rating = "Rating";
            public const string Source = "Source";
            public const string TrackNumber = "TrackNumber";
        }

        public SongTable()
        {
            AlbumId = 0;
            ArtistId = 0;
            Duration = 0;
            LastPlayed = 0;
            Name = string.Empty;
            Origin = SongOriginSource.Device;
            PlayCount = 0;
            Rating = 0;
            Source = string.Empty;
            TrackNumber = 0;
        }

        public SongTable(int albumId, int artistId, long duration, long lastPlayed, string name, 
            SongOriginSource origin, uint playCount, uint rating, string source, uint trackNumber)
        {
            AlbumId = albumId;
            ArtistId = artistId;
            Duration = duration;
            LastPlayed = lastPlayed;
            Name = name;
            Origin = origin;
            PlayCount = playCount;
            Rating = rating;
            Source = source;
            TrackNumber = trackNumber;
        }


        [Indexed]
        public int AlbumId { get; set; }

        [Indexed]
        public int ArtistId { get; set; }

        [Indexed]
        public string Source { get; set; }

        public long Duration { get; set; }

        public long LastPlayed { get; set; }

        public string Name { get; set; }

        public SongOriginSource Origin { get; set; }

        public uint PlayCount { get; set; }

        public uint Rating { get; set; }

        public uint TrackNumber { get; set; }
    }
}
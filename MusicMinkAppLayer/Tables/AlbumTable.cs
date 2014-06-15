using SQLite;

namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Defines the structure of the Album table for the SQLite database 
    /// </summary>
    [Table("AlbumTable")]
    public class AlbumTable : BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int AlbumId { get; set; }

        public static class Properties
        {
            public const string AlbumId = "AlbumId";

            public const string AlbumArt = "AlbumArt";
            public const string ArtistId = "ArtistId";
            public const string Name = "Name";
            public const string Year = "Year";
        }

        public AlbumTable()
        {
            AlbumArt = string.Empty;
            ArtistId = 0;
            Name = string.Empty;
            Year = 0;
        }

        public AlbumTable(string albumArt, int artistId, string name, uint year)
        {
            AlbumArt = albumArt;
            ArtistId = artistId;
            Name = name;
            Year = year;
        }

        [Indexed]
        public int ArtistId { get; set; }

        [Indexed]
        public string Name { get; set; }

        public string AlbumArt { get; set; }
        public uint Year { get; set; }
    }
}
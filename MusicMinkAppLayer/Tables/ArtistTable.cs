using SQLite;

namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Defines the structure of the Artist table for the SQLite database 
    /// </summary>
    [Table("ArtistTable")]
    public class ArtistTable : BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int ArtistId { get; set; }

        public static class Properties
        {
            public const string ArtistId = "ArtistId";

            public const string Name = "Name";
        }

        public ArtistTable()
        {
            Name = string.Empty;
        }

        public ArtistTable(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}

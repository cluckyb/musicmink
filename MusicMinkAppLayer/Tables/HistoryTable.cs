using SQLite;

namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Defines the structure of the History table for the SQLite database 
    /// 
    /// Used for tracking when songs are played so the can be properly scrobbled
    /// </summary>
    [Table("HistoryTable")]
    public class HistoryTable : BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int RowId { get; set; }

        public static class Properties
        {
            public const string RowId = "RowId";

            public const string SongId = "SongId";
            public const string Scrobbled = "Scrobbled";
            public const string Processed = "Processed";
            public const string DatePlayed = "DatePlayed";
        }

        public HistoryTable()
        {
            SongId = 0;
            Scrobbled = false;
            Processed = false;
            DatePlayed = 0;
        }

        public HistoryTable(int songId, bool scrobbled, bool processed, long datePlayed)
        {
            SongId = songId;
            Scrobbled = scrobbled;
            Processed = processed;
            DatePlayed = datePlayed;
        }

        public int SongId { get; set; }

        public bool Scrobbled { get; set; }

        public bool Processed { get; set; }

        public long DatePlayed { get; set; }
    }
}
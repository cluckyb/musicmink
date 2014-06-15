using SQLite;

namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Defines the structure of the PlayQueueEntry table for the SQLite database 
    /// 
    /// PlayQueue is stored as a indexed linked list with a value in ApplicationSettings tracking which row is active
    /// </summary>
    [Table("PlayQueueEntryTable")]
    public class PlayQueueEntryTable : BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int RowId { get; set; }

        public static class Properties
        {
            public const string RowId = "RowId";

            public const string SongId = "SongId";
            public const string NextId = "NextId";
            public const string PrevId = "PrevId";
        }        

        public PlayQueueEntryTable()
        {
            SongId = 0;
            NextId = 0;
            PrevId = 0;
        }

        public PlayQueueEntryTable(int song, int next, int prev)
        {
            SongId = song;
            NextId = next;
            PrevId = prev;
        }

        public int SongId { get; set; }

        public int NextId { get; set; }

        public int PrevId { get; set; }
    }
}

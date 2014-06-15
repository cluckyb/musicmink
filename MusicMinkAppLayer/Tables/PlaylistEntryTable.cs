using SQLite;

namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Defines the structure of the PlaylistEntry table for the SQLite database 
    /// 
    /// Playlist is stored by having a Playlist Table that tracks ID and Name, and then a linked list for each Playlist
    /// whose elements are all stored here
    /// </summary>
    [Table("PlaylistEntryTable")]
    public class PlaylistEntryTable : BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int RowId { get; set; }

        public static class Properties
        {
            public const string RowId = "RowId";

            public const string PlaylistId = "PlaylistId";
            public const string SongId = "SongId";
            public const string NextId = "NextId";
            public const string PrevId = "PrevId";
        }        

        public PlaylistEntryTable()
        {
            PlaylistId = 0;
            SongId = 0;
            NextId = 0;
            PrevId = 0;
        }

        public PlaylistEntryTable(int playlistId, int song, int next, int prev)
        {
            PlaylistId = playlistId;
            SongId = song;
            NextId = next;
            PrevId = prev;
        }

        [Indexed]
        public int PlaylistId { get; set; }

        public int SongId { get; set; }

        public int NextId { get; set; }

        public int PrevId { get; set; }
    }
}

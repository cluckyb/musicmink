using SQLite;

namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Defines the structure of the Playlist table for the SQLite database 
    ///
    /// Playlist is stored by having a this Playlist Table that tracks ID and Name, and then a linked list for each Playlist
    /// whose elements are all stored in the PlaylstEntry Table
    /// </summary>
    [Table("PlaylistTable")]
    public class PlaylistTable : BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int PlaylistId { get; set; }

        public static class Properties
        {
            public const string PlaylistId = "PlaylistId";

            public const string Name = "Name";
        }

        public PlaylistTable()
        {
            Name = string.Empty;
        }

        public PlaylistTable(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public bool IsHidden { get; set; }
    }
}

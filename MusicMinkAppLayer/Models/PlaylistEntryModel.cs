using MusicMinkAppLayer.Tables;

namespace MusicMinkAppLayer.Models
{
    /// <summary>
    /// Model for PlayQueueEntry entries
    /// </summary>
    public class PlaylistEntryModel : BaseModel<PlaylistEntryTable>
    {
        public static class Properties
        {
            public const string RowId = "RowId";

            public const string PlaylistId = "PlaylistId";
            public const string SongId = "SongId";
            public const string NextId = "NextId";
            public const string PrevId = "PrevId";
        }

        public PlaylistEntryModel(PlaylistEntryTable table)
            : base(table)
        {

        }

        public int RowId
        {
            get
            {
                return GetTableField<int>(PlaylistEntryTable.Properties.RowId);
            }
        }

        public int PlaylistId
        {
            get
            {
                return GetTableField<int>(PlaylistEntryTable.Properties.PlaylistId);
            }
            set
            {
                SetTableField<int>(PlaylistEntryTable.Properties.PlaylistId, value, Properties.PlaylistId);
            }
        }

        public int SongId
        {
            get
            {
                return GetTableField<int>(PlaylistEntryTable.Properties.SongId);
            }
            set
            {
                SetTableField<int>(PlaylistEntryTable.Properties.SongId, value, Properties.SongId);
            }
        }


        public int NextId
        {
            get
            {
                return GetTableField<int>(PlaylistEntryTable.Properties.NextId);
            }
            set
            {
                SetTableField<int>(PlaylistEntryTable.Properties.NextId, value, Properties.NextId);
            }
        }


        public int PrevId
        {
            get
            {
                return GetTableField<int>(PlaylistEntryTable.Properties.PrevId);
            }
            set
            {
                SetTableField<int>(PlaylistEntryTable.Properties.PrevId, value, Properties.PrevId);
            }
        }
    }
}

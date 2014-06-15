using MusicMinkAppLayer.Tables;

namespace MusicMinkAppLayer.Models
{
    /// <summary>
    /// Model for PlayQueueEntry entries
    /// </summary>
    public class PlayQueueEntryModel : BaseModel<PlayQueueEntryTable>
    {
        public static class Properties
        {
            public const string RowId = "RowId";

            public const string SongId = "SongId";
            public const string NextId = "NextId";
            public const string PrevId = "PrevId";
        }

        public PlayQueueEntryModel(PlayQueueEntryTable table)
            : base(table)
        {

        }

        public int RowId
        {
            get
            {
                return GetTableField<int>(PlayQueueEntryTable.Properties.RowId);
            }
        }

        public int SongId
        {
            get
            {
                return GetTableField<int>(PlayQueueEntryTable.Properties.SongId);
            }
            set
            {
                SetTableField<int>(PlayQueueEntryTable.Properties.SongId, value, Properties.SongId);
            }
        }


        public int NextId
        {
            get
            {
                return GetTableField<int>(PlayQueueEntryTable.Properties.NextId);
            }
            set
            {
                SetTableField<int>(PlayQueueEntryTable.Properties.NextId, value, Properties.NextId);
            }
        }


        public int PrevId
        {
            get
            {
                return GetTableField<int>(PlayQueueEntryTable.Properties.PrevId);
            }
            set
            {
                SetTableField<int>(PlayQueueEntryTable.Properties.PrevId, value, Properties.PrevId);
            }
        }
    }
}

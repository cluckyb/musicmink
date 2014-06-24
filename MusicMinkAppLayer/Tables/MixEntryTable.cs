using MusicMinkAppLayer.Enums;
using SQLite;


namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Defines the structure of the MixEntry table for the SQLite database 
    /// 
    /// Mixes are stored by having a Mix Table that tracks ID, Name, Root Mix Entry... and then all the actual entries are stored here
    /// </summary>
    [Table("MixEntryTable")]
    public class MixEntryTable : BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int EntryId { get; set; }

        public static class Properties
        {
            public const string EntryId = "EntryId";
            public const string MixId = "MixId";

            public const string Type = "Type";
            public const string Input = "Input";
            public const string IsRoot = "IsRoot";
        }

        public MixEntryTable()
        {
            MixId = 0;
            Input = string.Empty;
            IsRoot = false;
            Type = MixType.None;
        }

        public MixEntryTable(int mixId, string input, bool isRoot, MixType type)
        {
            MixId = mixId;
            Input = input;
            IsRoot = isRoot;
            Type = type;
        }

        [Indexed]
        public int MixId { get; set; }

        public string Input { get; set; }

        public bool IsRoot { get; set; }

        public MixType Type { get; set; }
    }
}

using MusicMinkAppLayer.Enums;
using SQLite;

namespace MusicMinkAppLayer.Tables
{
    /// <summary>
    /// Defines the structure of the Mix table for the SQLite database 
    /// 
    /// Mixes are stored by having a Mix Table that tracks ID, Name, Root Mix Entry... and then all the structure is stored in the mix entry table
    /// </summary>
    [Table("MixTable")]
    public class MixTable : BaseTable
    {
        [PrimaryKey, AutoIncrement]
        public int MixId { get; set; }

        public static class Properties
        {
            public const string MixId = "MixId";

            public const string Name = "Name";
            public const string IsHidden = "IsHidden";

            public const string HasLimit = "HasLimit";
            public const string Limit = "Limit";
            public const string SortType = "SortType";
        }

        public MixTable()
        {
            Name = string.Empty;
            IsHidden = false;
            Limit = 0;
            SortType = MixSortOrder.None;
            HasLimit = false;
        }

        public MixTable(string name, bool isHidden, uint limit, MixSortOrder sortType, bool hasLimit)
        {
            Name = name;
            IsHidden = isHidden;
            Limit = limit;
            SortType = sortType;
            HasLimit = hasLimit;
        }

        public string Name { get; set; }

        public bool IsHidden { get; set; }

        public uint Limit { get; set; }

        public MixSortOrder SortType { get; set; }

        public bool HasLimit { get; set; }
    }
}

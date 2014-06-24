using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Tables;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MusicMinkAppLayer.Models
{
    public class MixModel : BaseModel<MixTable>
    {
        private Dictionary<int, MixEntryModel> indexLookupMap = new Dictionary<int, MixEntryModel>();

        public static class Properties
        {
            public const string MixId = "MixId";

            public const string Name = "Name";
            public const string IsHidden = "IsHidden";

            public const string Limit = "Limit";
            public const string SortType = "SortType";
            public const string HasLimit = "HasLimit";

            public const string RootMixEntry = "RootMixEntry";
        }

        public MixModel(MixTable table)
            : base(table)
        {

        }

        #region Properties

        public int MixId
        {
            get
            {
                return GetTableField<int>(MixTable.Properties.MixId);
            }
        }

        public string Name
        {
            get
            {
                return GetTableField<string>(MixTable.Properties.Name);
            }
            set
            {
                SetTableField<string>(MixTable.Properties.Name, value, Properties.Name);
            }
        }

        public MixSortOrder SortType
        {
            get
            {
                return GetTableField<MixSortOrder>(MixTable.Properties.SortType);
            }
            set
            {
                SetTableField<MixSortOrder>(MixTable.Properties.SortType, value, Properties.SortType);
            }
        }

        public uint Limit
        {
            get
            {
                return GetTableField<uint>(MixTable.Properties.Limit);
            }
            set
            {
                SetTableField<uint>(MixTable.Properties.Limit, value, Properties.Limit);
            }
        }

        public bool HasLimit
        {
            get
            {
                return GetTableField<bool>(MixTable.Properties.HasLimit);
            }
            set
            {
                SetTableField<bool>(MixTable.Properties.HasLimit, value, Properties.HasLimit);
            }
        }

        public bool IsHidden
        {
            get
            {
                return GetTableField<bool>(MixTable.Properties.IsHidden);
            }
            set
            {
                SetTableField<bool>(MixTable.Properties.IsHidden, value, Properties.IsHidden);
            }
        }

        private MixEntryModel _rootMixEntry;
        public MixEntryModel RootMixEntry
        {
            get
            {
                return _rootMixEntry;
            }
            set
            {
                if (_rootMixEntry != value)
                {
                    _rootMixEntry = value;
                    NotifyPropertyChanged(Properties.RootMixEntry);
                }
            }
        }

        #endregion

        #region Methods

        internal void Populate()
        {
            List<MixEntryTable> allEntries = DatabaseManager.Current.FetchMixEntriesForMix(MixId);

            foreach (MixEntryTable mixEntry in allEntries)
            {
                AddEntry(mixEntry);
            }
        }

        internal void AddEntry(MixEntryTable mixEntry)
        {
            MixEntryModel newEntry = new MixEntryModel(mixEntry);

            indexLookupMap.Add(newEntry.EntryId, newEntry);

            if (newEntry.IsRoot)
            {
                DebugHelper.Assert(new CallerInfo(), RootMixEntry == null, "Ran into two root mix entries for the same mix id");
                RootMixEntry = newEntry;
            }
        }

        public MixEntryModel LookupMixEntry(int entryId)
        {
            if (indexLookupMap.ContainsKey(entryId))
            {
                return indexLookupMap[entryId];
            }
            else
            {
                return null;
            }
        }

        #endregion

        public void ClearMixEntries()
        {
            indexLookupMap.Clear();
            RootMixEntry = null;

            DatabaseManager.Current.DeleteAllMixEntries(MixId);
        }

        public void UpdateMixEntries()
        {
            Populate();
        }
    }
}

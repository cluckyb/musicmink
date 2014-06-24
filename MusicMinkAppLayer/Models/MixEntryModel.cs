using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Tables;

namespace MusicMinkAppLayer.Models
{
    public class MixEntryModel : BaseModel<MixEntryTable>
    {
        public static class Properties
        {
            public const string EntryId = "EntryId";
            public const string MixId = "MixId";

            public const string Type = "Type";
            public const string Input = "Input";
            public const string IsRoot = "IsRoot";
        }

        public MixEntryModel(MixEntryTable table)
            : base(table)
        {

        }

        public int EntryId
        {
            get
            {
                return GetTableField<int>(MixEntryTable.Properties.EntryId);
            }
        }

        public int MixId
        {
            get
            {
                return GetTableField<int>(MixEntryTable.Properties.MixId);
            }
        }

        public bool IsRoot
        {
            get
            {
                return GetTableField<bool>(MixEntryTable.Properties.IsRoot);
            }
        }

        public MixType Type
        {
            get
            {
                return GetTableField<MixType>(MixEntryTable.Properties.Type);
            }
            set
            {
                SetTableField<MixType>(MixEntryTable.Properties.Type, value, Properties.Type);
            }
        }

        public string Input
        {
            get
            {
                return GetTableField<string>(MixEntryTable.Properties.Input);
            }
            set
            {
                SetTableField<string>(MixEntryTable.Properties.Input, value, Properties.Input);
            }
        }

        public static int SaveMix(MixType mixType, string input, int mixId, bool isRoot)
        {
            MixEntryTable newEntry = new MixEntryTable(mixId, input, isRoot, mixType);
            return DatabaseManager.Current.AddMixEntry(newEntry);
        }

        /*
         * 
         *         public static MixModel LookupMix(int id)
        {
            MixTable mixLookup = DB.SelectMixById(DB.mainDB, id).FirstOrDefault<MixTable>();

            if (mixLookup == null)
            {
                return null;
            }
            else
            {
                return new MixModel(mixLookup);
            }
        }

        public static string SaveMix(int Id, MixType Type, string Input, int rootListId)
        {
            MixModel lookup = LookupMix(Id);

            if (lookup != null)
            {
                lookup.Type = Type;
                lookup.Input = Input;

                return lookup.Id.ToString();
            }
            else
            {
                MixTable mix = new MixTable() { Type = Type, Input = Input, IsRoot = false, MixListId = rootListId};
                DB.mainDB.Mixes.InsertOnSubmit(mix);
                DB.mainDB.SubmitChanges();

                return mix.MixId.ToString();
            }
        }*/



    }
}

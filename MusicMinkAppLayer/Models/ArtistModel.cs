using MusicMinkAppLayer.Tables;

namespace MusicMinkAppLayer.Models
{
    /// <summary>
    /// Model for Artist entries
    /// </summary>
    public class ArtistModel : BaseModel<ArtistTable>
    {
        public static class Properties
        {
            public const string ArtistId = "ArtistId";

            public const string Name = "Name";
        }

        public ArtistModel(ArtistTable table) : base(table)
        {

        }

        public int ArtistId
        {
            get
            {
                return GetTableField<int>(ArtistTable.Properties.ArtistId);
            }
        }

        public string Name
        {
            get
            {
                return GetTableField<string>(ArtistTable.Properties.Name);
            }
            set
            {
                SetTableField<string>(ArtistTable.Properties.Name, value, Properties.Name);
            }
        }
    }
}

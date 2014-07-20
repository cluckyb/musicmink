using MusicMinkAppLayer.Tables;
using System;
using Windows.Storage;

namespace MusicMinkAppLayer.Models
{
    /// <summary>
    /// Model for Album entries
    /// </summary>
    public class AlbumModel : BaseModel<AlbumTable>
    {
        public static class Properties
        {
            public const string AlbumId = "AlbumId";

            public const string AlbumArt = "AlbumArt";
            public const string ArtistId = "ArtistId";
            public const string Name = "Name";
            public const string Year = "Year";
        }

        public AlbumModel(AlbumTable table)
            : base(table)
        {

        }

        public int AlbumId
        {
            get
            {
                return GetTableField<int>(AlbumTable.Properties.AlbumId);
            }
        }

        public string AlbumArt
        {
            get
            {
                return GetTableField<string>(AlbumTable.Properties.AlbumArt);
            }
            set
            {
                SetTableField<string>(AlbumTable.Properties.AlbumArt, value, Properties.AlbumArt);
            }
        }

        public int ArtistId
        {
            get
            {
                return GetTableField<int>(AlbumTable.Properties.ArtistId);
            }
            set
            {
                SetTableField<int>(AlbumTable.Properties.ArtistId, value, Properties.ArtistId);
            }
        }

        public string Name
        {
            get
            {
                return GetTableField<string>(AlbumTable.Properties.Name);
            }
            set
            {
                SetTableField<string>(AlbumTable.Properties.Name, value, Properties.Name);
            }
        }

        public uint Year
        {
            get
            {
                return GetTableField<uint>(AlbumTable.Properties.Year);
            }
            set
            {
                SetTableField<uint>(AlbumTable.Properties.Year, value, Properties.Year);
            }
        }

        internal async void DeleteArt()
        {
            if (!string.IsNullOrEmpty(AlbumArt))
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(AlbumArt);

                await file.DeleteAsync();
            }
        }
    }
}

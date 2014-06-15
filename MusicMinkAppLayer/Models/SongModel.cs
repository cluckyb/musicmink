using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Tables;

namespace MusicMinkAppLayer.Models
{
    /// <summary>
    /// Model for Song Entries
    /// </summary>
    public class SongModel : BaseModel<SongTable>
    {
        public static class Properties
        {
            public const string SongId = "SongId";

            public const string AlbumId = "AlbumId";
            public const string ArtistId = "ArtistId";
            public const string Duration = "Duration";
            public const string LastPlayed = "LastPlayed";
            public const string Name = "Name";
            public const string Origin = "Origin";
            public const string PlayCount = "PlayCount";
            public const string Rating = "Rating";
            public const string Source = "Source";
            public const string TrackNumber = "TrackNumber";
        }

        internal SongModel(SongTable table)
            : base(table)
        {

        }

        public int SongId
        {
            get
            {
                return GetTableField<int>(SongTable.Properties.SongId);
            }
        }

        public int AlbumId
        {
            get
            {
                return GetTableField<int>(SongTable.Properties.AlbumId);
            }
            set
            {
                SetTableField<int>(SongTable.Properties.AlbumId, value, Properties.AlbumId);
            }
        }

        public int ArtistId
        {
            get
            {
                return GetTableField<int>(SongTable.Properties.ArtistId);
            }
            set
            {
                SetTableField<int>(SongTable.Properties.ArtistId, value, Properties.ArtistId);
            }
        }

        public long Duration
        {
            get
            {
                return GetTableField<long>(SongTable.Properties.Duration);
            }
            set
            {
                SetTableField<long>(SongTable.Properties.Duration, value, Properties.Duration);
            }
        }

        public long LastPlayed
        {
            get
            {
                return GetTableField<long>(SongTable.Properties.LastPlayed);
            }
            set
            {
                SetTableField<long>(SongTable.Properties.LastPlayed, value, Properties.LastPlayed);
            }
        }

        public string Name
        {
            get
            {
                return GetTableField<string>(SongTable.Properties.Name);
            }
            set
            {
                SetTableField<string>(SongTable.Properties.Name, value, Properties.Name);
            }
        }

        public SongOriginSource Origin
        {
            get
            {
                return GetTableField<SongOriginSource>(SongTable.Properties.Origin);
            }
            set
            {
                SetTableField<SongOriginSource>(SongTable.Properties.Origin, value, Properties.Origin);
            }
        }

        public uint PlayCount
        {
            get
            {
                return GetTableField<uint>(SongTable.Properties.PlayCount);
            }
            set
            {
                SetTableField<uint>(SongTable.Properties.PlayCount, value, Properties.PlayCount);
            }
        }

        public uint Rating
        {
            get
            {
                return GetTableField<uint>(SongTable.Properties.Rating);
            }
            set
            {
                SetTableField<uint>(SongTable.Properties.Rating, value, Properties.Rating);
            }
        }

        public string Source
        {
            get
            {
                return GetTableField<string>(SongTable.Properties.Source);
            }
            set
            {
                SetTableField<string>(SongTable.Properties.Source, value, Properties.Source);
            }
        }

        public uint TrackNumber
        {
            get
            {
                return GetTableField<uint>(SongTable.Properties.TrackNumber);
            }
            set
            {
                SetTableField<uint>(SongTable.Properties.TrackNumber, value, Properties.TrackNumber);
            }
        }
    }
}

using MusicMinkAppLayer.Models;
using System.ComponentModel;

namespace MusicMink.ViewModels
{
    public class PlaylistEntryViewModel : BaseViewModel<PlaylistEntryModel>
    {
        public static class Properties
        {
            public const string RowId = "RowId";
            public const string Song = "Song";
        }

        public PlaylistEntryViewModel(PlaylistEntryModel model)
            : base(model)
        {
            model.PropertyChanged += HandleModelPropertyChanged;
        }

        void HandleModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case PlaylistEntryModel.Properties.RowId:
                    _song = null;
                    NotifyPropertyChanged(Properties.RowId);
                    NotifyPropertyChanged(Properties.Song);
                    break;
            }
        }

        public int RowId
        {
            get
            {
                return rootModel.RowId;
            }
        }

        private SongViewModel _song;
        public SongViewModel Song
        {
            get
            {
                if (_song == null)
                {
                   _song = LibraryViewModel.Current.LookupSongById(rootModel.SongId);
                }

                return _song;
            }
        }

        private PlaylistViewModel _playlist;
        public PlaylistViewModel Playlist
        {
            get
            {
                if (_playlist == null)
                {
                    _playlist = LibraryViewModel.Current.LookupPlaylistById(rootModel.PlaylistId);
                }

                return _playlist;
            }
        }
    }
}

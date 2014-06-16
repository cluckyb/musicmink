using MusicMink.Collections;
using MusicMink.Common;
using MusicMink.Dialogs;
using MusicMinkAppLayer.Models;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MusicMink.ViewModels
{
    public class PlaylistViewModel : BaseViewModel<PlaylistModel>
    {
        public static class Properties
        {
            public const string PlaylistId = "PlaylistId";

            public const string Name = "Name";
            public const string LengthInfo = "LengthInfo";
            public const string Songs = "Songs";

            public const string IsBeingDeleted = "IsBeingDeleted";
        }

        public PlaylistViewModel(PlaylistModel model)
            : base(model)
        {
            _songs = new ObservableCopyCollection<PlaylistEntryViewModel, PlaylistEntryModel>(model.Songs,
                (playlistEntryModel) => { return new PlaylistEntryViewModel(playlistEntryModel); });

            Songs.CollectionChanged += HandleSongsCollectionChanged;
            model.PropertyChanged += HandleRootModelPropertyChanged;
        }

        #region Event Handlers

        int oldIndex;
        void HandleSongsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(Properties.LengthInfo);

            if (!Songs.IsRootChanging)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    oldIndex = e.OldStartingIndex;
                }
                else if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    int newIndex = e.NewStartingIndex;

                    Songs.IsEndChanging = true;

                    rootModel.MoveSong(oldIndex, newIndex);

                    Songs.IsEndChanging = false;
                }
            }

            QueueAllSongs.RaiseExecuteChanged();
            PlayAllSongs.RaiseExecuteChanged();
            ShuffleAllSongs.RaiseExecuteChanged();
        }

        void HandleRootModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case PlaylistModel.Properties.Name:
                    NotifyPropertyChanged(Properties.Name);
                    break;
            }
        }

        #endregion

        #region Properties

        public int PlaylistId
        {
            get
            {
                return rootModel.PlaylistId;
            }
        }

        private bool _isBeingDeleted = false;
        public bool IsBeingDeleted
        {
            get
            {
                return _isBeingDeleted;
            }
            set
            {
                if (_isBeingDeleted != value)
                {
                    _isBeingDeleted = value;
                    NotifyPropertyChanged(Properties.IsBeingDeleted);
                }
            }
        }

        public string LengthInfo
        {
            get
            {
                return Strings.HandlePlural(Songs.Count, Strings.GetResource("FormatSongsPlural"), Strings.GetResource("FormatSongsSingular"));
            }
        }

        public string Name
        {
            get
            {
                if (rootModel.Name == string.Empty) return Strings.GetResource("UnknownPlaylistString");
                return rootModel.Name;
            }
            set
            {
                if (rootModel.Name != value)
                {
                    rootModel.Name = value;
                }
            }
        }

        private ObservableCopyCollection<PlaylistEntryViewModel, PlaylistEntryModel> _songs;
        public ObservableCopyCollection<PlaylistEntryViewModel, PlaylistEntryModel> Songs
        {
            get
            {
                return _songs;
            }
        }

        #endregion

        #region Commands

        private RelayCommand _playAllSongs;
        public RelayCommand PlayAllSongs
        {
            get
            {
                if (_playAllSongs == null) _playAllSongs = new RelayCommand(CanExecutePlayAllSongs, ExecutePlayAllSongs);

                return _playAllSongs;
            }
        }

        private void ExecutePlayAllSongs(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.PlaySongList(Songs, (playlistEntry) => { return playlistEntry.Song; }, true);
        }

        private bool CanExecutePlayAllSongs(object parameter)
        {
            return Songs.Count > 0;
        }


        private RelayCommand _queueAllSongs;
        public RelayCommand QueueAllSongs
        {
            get
            {
                if (_queueAllSongs == null) _queueAllSongs = new RelayCommand(CanExecuteQueueAllSongs, ExecuteQueueAllSongs);

                return _queueAllSongs;
            }
        }

        private void ExecuteQueueAllSongs(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.PlaySongList(Songs, (playlistEntry) => { return playlistEntry.Song; }, false);
        }

        private bool CanExecuteQueueAllSongs(object parameter)
        {
            return Songs.Count > 0;
        }


        private RelayCommand _shuffleAllSongs;
        public RelayCommand ShuffleAllSongs
        {
            get
            {
                if (_shuffleAllSongs == null) _shuffleAllSongs = new RelayCommand(CanExecuteShuffleAllSongs, ExecuteShuffleAllSongs);

                return _shuffleAllSongs;
            }
        }

        private void ExecuteShuffleAllSongs(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.ShuffleSongList(Songs, (playlistEntry) => { return playlistEntry.Song; }, true);
        }

        private bool CanExecuteShuffleAllSongs(object parameter)
        {
            return Songs.Count > 0;
        }


        private RelayCommand _editPlaylist;
        public RelayCommand EditPlaylist
        {
            get
            {
                if (_editPlaylist == null) _editPlaylist = new RelayCommand(CanExecuteEditPlaylist, ExecuteEditPlaylist);

                return _editPlaylist;
            }
        }

        private async void ExecuteEditPlaylist(object parameter)
        {
            EditPlaylist editPlaylistDialog = new EditPlaylist(this);

            await editPlaylistDialog.ShowAsync();
        }

        private bool CanExecuteEditPlaylist(object parameter)
        {
            return true;
        }
        #endregion

        #region Methods

        public void AddSong(SongViewModel songToAdd)
        {
            rootModel.AddSong(songToAdd.SongId);
        }

        internal void RemoveAllInstancesOfSong(SongViewModel song)
        {
            rootModel.RemoveSong(song.SongId);
        }

        #endregion
    }
}

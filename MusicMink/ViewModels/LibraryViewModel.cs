using MusicMink.Collections;
using MusicMink.Common;
using MusicMink.Dialogs;
using MusicMink.MediaSources;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MusicMink.ViewModels
{
    class LibraryViewModel : INotifyPropertyChanged
    {
        private static LibraryViewModel _current;
        public static LibraryViewModel Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new LibraryViewModel();
                }

                return _current;
            }
        }

        PerfTracer perf = new PerfTracer("Load LibraryViewModel");

        private LibraryViewModel()
        {
            perf.Restart();

            LibraryModel.Current.Start();

            perf.Trace("Model created");

            LibraryModel.Current.AllSongs.CollectionChanged += HandleAllSongsCollectionChanged;
            LibraryModel.Current.Playlists.CollectionChanged += HandlePlaylistsCollectionChanged;

            LibraryModel.Current.AlbumCreated += HandleLibraryModelAlbumCreated;

            _playQueue = new PlayQueueViewModel(LibraryModel.Current.PlayQueue);

            perf.Trace("first pass load done");
        }

        ~LibraryViewModel()
        {
            LibraryModel.Current.Playlists.CollectionChanged -= HandlePlaylistsCollectionChanged;
            LibraryModel.Current.AllSongs.CollectionChanged -= HandleAllSongsCollectionChanged;
        }

        // Delayed constructor for once LibraryModel is loaded
        public void Initalize()
        {
            perf.Trace("Initalize Started");

            foreach (SongModel newSong in LibraryModel.Current.AllSongs)
            {
                LookupSong(newSong);
            }

            perf.Trace("Songs loaded");

            foreach (PlaylistModel newPlaylist in LibraryModel.Current.Playlists)
            {
                LookupPlaylist(newPlaylist);
            }

            perf.Trace("Playlists loaded");

            PlayQueue.Initalize();

            perf.Trace("Play queue loaded");
        }

        # region Event Handlers

        private async void HandleLibraryModelAlbumCreated(object sender, AlbumCreatedEventArgs e)
        {
            if (SettingsViewModel.Current.AutoPullArtFromLastFM && string.IsNullOrEmpty(e.NewAlbum.AlbumArt))
            {
                AlbumViewModel newViewModel = LookupAlbum(e.NewAlbum);

                await newViewModel.SetArtToLastFM(false);
            }
        }

        private void HandlePlaylistsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (PlaylistModel model in e.OldItems)
                {
                    PlaylistCollection.Remove(PlaylistLookupMap[model.PlaylistId]);
                    PlaylistLookupMap.Remove(model.PlaylistId);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (PlaylistModel model in e.NewItems)
                {
                    LookupPlaylist(model);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                PlaylistCollection.Clear();
                PlaylistLookupMap.Clear();
            }
        }

        private void HandleAllSongsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (SongViewModel oldSongViewModel in FlatSongCollection)
                {
                    oldSongViewModel.Album.Songs.Remove(oldSongViewModel);
                    oldSongViewModel.Artist.Songs.Remove(oldSongViewModel);

                    SongCollection.Remove(oldSongViewModel, oldSongViewModel.SortName);
                    FlatSongCollection.Remove(oldSongViewModel);
                    SongLookupMap.Remove(oldSongViewModel.SongId);
                }
            }
            else
            {
                if (e.OldItems != null)
                {
                    foreach (SongModel oldSong in e.OldItems)
                    {
                        SongViewModel oldSongViewModel = LookupSong(oldSong);

                        SongCollection.Remove(oldSongViewModel, oldSongViewModel.SortName);
                        FlatSongCollection.Remove(oldSongViewModel);
                        SongLookupMap.Remove(oldSongViewModel.SongId);

                        oldSongViewModel.Album.Songs.Remove(oldSongViewModel);
                        oldSongViewModel.Artist.Songs.Remove(oldSongViewModel);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (SongModel newSong in e.NewItems)
                    {
                        LookupSong(newSong);
                    }
                }
            }
        }

        #endregion

        #region Commands

        private RelayCommand _addNewPlaylist;
        public RelayCommand AddNewPlaylist
        {
            get
            {
                if (_addNewPlaylist == null) _addNewPlaylist = new RelayCommand(CanExecuteAddNewPlaylist, ExecuteAddNewPlaylist);

                return _addNewPlaylist;
            }
        }

        private async void ExecuteAddNewPlaylist(object parameter)
        {
            AddPlaylist addPlaylistDialog = new AddPlaylist();

            await addPlaylistDialog.ShowAsync();
        }

        private bool CanExecuteAddNewPlaylist(object parameter)
        {
            return true;
        }

        #endregion

        #region Collections

        private Dictionary<int, ArtistViewModel> ArtistLookupMap = new Dictionary<int, ArtistViewModel>();
        private Dictionary<int, AlbumViewModel> AlbumLookupMap = new Dictionary<int, AlbumViewModel>();
        private Dictionary<int, SongViewModel> SongLookupMap = new Dictionary<int, SongViewModel>();
        private Dictionary<int, PlaylistViewModel> PlaylistLookupMap = new Dictionary<int, PlaylistViewModel>();

        private AlphaGroupedObservableCollection<ArtistViewModel> _artistCollection = new AlphaGroupedObservableCollection<ArtistViewModel>();
        public AlphaGroupedObservableCollection<ArtistViewModel> ArtistCollection
        {
            get
            {
                return _artistCollection;
            }
        }

        private AlphaGroupedObservableCollection<AlbumViewModel> _albumCollection = new AlphaGroupedObservableCollection<AlbumViewModel>();
        public AlphaGroupedObservableCollection<AlbumViewModel> AlbumCollection
        {
            get
            {
                return _albumCollection;
            }
        }

        private ObservableCollection<SongViewModel> _flatSongCollection = new ObservableCollection<SongViewModel>();
        public ObservableCollection<SongViewModel> FlatSongCollection
        {
            get
            {
                return _flatSongCollection;
            }
        }

        private AlphaGroupedObservableCollection<SongViewModel> _songCollection = new AlphaGroupedObservableCollection<SongViewModel>();
        public AlphaGroupedObservableCollection<SongViewModel> SongCollection
        {
            get
            {
                return _songCollection;
            }
        }

        private ObservableCollection<PlaylistViewModel> _playlistCollection = new ObservableCollection<PlaylistViewModel>();
        public ObservableCollection<PlaylistViewModel> PlaylistCollection
        {
            get
            {
                return _playlistCollection;
            }
        }

        private PlayQueueViewModel _playQueue;
        public PlayQueueViewModel PlayQueue
        {
            get
            {
                return _playQueue;
            }
        }

        #endregion

        #region Functions

        public ArtistViewModel LookupArtistById(int artistId)
        {
            // check the cache, its probably there
            if (ArtistLookupMap.ContainsKey(artistId))
            {
                return ArtistLookupMap[artistId];
            }
            else
            {
                // If not, see if its at least in the database
                return LookupArtist(LibraryModel.Current.LookupArtistById(artistId));
            }
        }

        private ArtistViewModel LookupArtist(ArtistModel artist)
        {
            if (artist == null) return null;

            if (ArtistLookupMap.ContainsKey(artist.ArtistId))
            {
                return ArtistLookupMap[artist.ArtistId];
            }
            else
            {
                ArtistViewModel newArtistViewModel = new ArtistViewModel(artist);

                ArtistLookupMap.Add(newArtistViewModel.ArtistId, newArtistViewModel);
                ArtistCollection.Add(newArtistViewModel, newArtistViewModel.SortName);

                return newArtistViewModel;
            }
        }

        public ArtistViewModel LookupArtistByName(string name)
        {
            return LookupArtist(LibraryModel.Current.LookupArtistByName(name));
        }

        internal void RemoveArtistIfNeeded(ArtistViewModel artistViewModel)
        {
            if (artistViewModel.Songs.Count == 0 && artistViewModel.Albums.Count == 0)
            {
                ArtistCollection.Remove(artistViewModel, artistViewModel.SortName);
                ArtistLookupMap.Remove(artistViewModel.ArtistId);

                artistViewModel.IsBeingDeleted = true;

                LibraryModel.Current.DeleteArtist(artistViewModel.ArtistId);
            }
        }

        public AlbumViewModel LookupAlbumById(int albumId)
        {
            // check the cache, its probably there
            if (AlbumLookupMap.ContainsKey(albumId))
            {
                return AlbumLookupMap[albumId];
            }
            else
            {
                // If not, see if its at least in the database
                return LookupAlbum(LibraryModel.Current.LookupAlbumById(albumId));
            }
        }

        private AlbumViewModel LookupAlbum(AlbumModel album)
        {
            if (AlbumLookupMap.ContainsKey(album.AlbumId))
            {
                return AlbumLookupMap[album.AlbumId];
            }
            else
            {
                ArtistViewModel artist = LookupArtistById(album.ArtistId);

                AlbumViewModel newAlbumViewModel = new AlbumViewModel(album, artist);

                AlbumLookupMap.Add(newAlbumViewModel.AlbumId, newAlbumViewModel);
                AlbumCollection.Add(newAlbumViewModel, newAlbumViewModel.SortName);
                return newAlbumViewModel;
            }
        }

        public AlbumViewModel LookupAlbumByName(string name, int artistId)
        {
            return LookupAlbum(LibraryModel.Current.LookupAlbumByName(name, artistId));
        }

        public AlbumViewModel AlbumSearch(string albumName, string albumArtistName)
        {
            ArtistModel artist = LibraryModel.Current.SearchArtistByName(albumArtistName);

            if (artist == null) return null;

            AlbumModel album = LibraryModel.Current.SearchAlbumByName(albumName, artist.ArtistId);

            if (album == null) return null;
            else return LookupAlbum(album);
        }

        internal void RemoveAlbumIfNeeded(AlbumViewModel albumViewModel)
        {
            if (albumViewModel.Songs.Count == 0)
            {
                if (albumViewModel.Artist.Albums.Contains(albumViewModel))
                {
                    albumViewModel.Artist.Albums.Remove(albumViewModel);
                    RemoveArtistIfNeeded(albumViewModel.Artist);

                    AlbumCollection.Remove(albumViewModel, albumViewModel.SortName);
                    AlbumLookupMap.Remove(albumViewModel.AlbumId);

                    albumViewModel.IsBeingDeleted = true;

                    LibraryModel.Current.DeleteAlbum(albumViewModel.AlbumId);
                }
            }
        }

        public SongViewModel LookupSongById(int songId)
        {
            if (songId <= 0) return null;

            // check the cache, its probably there
            if (SongLookupMap.ContainsKey(songId))
            {
                return SongLookupMap[songId];
            }
            else
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Couldn't find song {0} in cache", songId);
                // If not, see if its at least in the database
                return LookupSong(LibraryModel.Current.LookupSongById(songId));
            }
        }

        internal SongViewModel LookupSongByName(string TrackName, string ArtistName)
        {
            ArtistViewModel artist = LookupArtistByName(ArtistName);

            if (artist == null)
            {
                return null;
            }

            return LookupSong(LibraryModel.Current.SearchSongByName(TrackName, artist.ArtistId));
        }


        private SongViewModel LookupSong(SongModel song)
        {
            if (song == null) return null;

            if (SongLookupMap.ContainsKey(song.SongId))
            {
                return SongLookupMap[song.SongId];
            }
            else
            {
                ArtistViewModel artist = LookupArtistById(song.ArtistId);

                AlbumViewModel album = LookupAlbumById(song.AlbumId);

                SongViewModel newSongViewModel = new SongViewModel(song, artist, album);

                SongLookupMap.Add(newSongViewModel.SongId, newSongViewModel);
                SongCollection.Add(newSongViewModel, newSongViewModel.SortName);
                FlatSongCollection.Add(newSongViewModel);

                return newSongViewModel;
            }
        }

        internal bool AddSong(StorageProviderSong song)
        {
            if (LibraryModel.Current.DoesSongExist(StorageProviderSourceToSongOriginSource(song.Origin), song.Source))
            {
                return false;
            }

            SongModel newSongModel = LibraryModel.Current.AddNewSong(
                song.Artist,
                song.Album,
                song.AlbumArtist,
                song.Name,
                song.Source,
                StorageProviderSourceToSongOriginSource(song.Origin),
                song.Duration.Ticks,
                song.Rating,
                song.TrackNumber
                );

            if (LookupSong(newSongModel) == null)
            {
                DebugHelper.Alert(new CallerInfo(), "Failed to add song");
                return false;
            }

            return true;
        }

        private SongOriginSource StorageProviderSourceToSongOriginSource(StorageProviderSource sps)
        {
            switch (sps)
            {
                case StorageProviderSource.Device:
                    return SongOriginSource.Device;
                case StorageProviderSource.OneDrive:
                    return SongOriginSource.OneDrive;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected StorageProviderSource {0}", sps);
                    return SongOriginSource.Unknown;
            }

        }



        public PlaylistViewModel LookupPlaylistById(int playlistId)
        {
            // check the cache, its probably there
            if (PlaylistLookupMap.ContainsKey(playlistId))
            {
                return PlaylistLookupMap[playlistId];
            }
            else
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Couldn't find playlist {0} in cache", playlistId);

                return null;
            }
        }

        public PlaylistViewModel LookupPlaylist(PlaylistModel playlist)
        {
            if (PlaylistLookupMap.ContainsKey(playlist.PlaylistId))
            {
                return PlaylistLookupMap[playlist.PlaylistId];
            }
            else
            {
                PlaylistViewModel newViewModel = new PlaylistViewModel(playlist);

                PlaylistLookupMap.Add(newViewModel.PlaylistId, newViewModel);
                PlaylistCollection.Add(newViewModel);
                return newViewModel;
            }
        }

        #endregion

        #region Methods

        internal void AddPlaylist(string p)
        {
            LibraryModel.Current.AddPlaylist(p);
        }

        internal void DeletePlaylist(PlaylistViewModel playlist)
        {
            playlist.IsBeingDeleted = true;

            LibraryModel.Current.DeletePlaylist(playlist.PlaylistId);
        }


        internal void AlertAlbumNameChanged(AlbumViewModel albumViewModel, string oldName)
        {
            AlbumCollection.Remove(albumViewModel, oldName);
            AlbumCollection.Add(albumViewModel, albumViewModel.SortName);
        }

        internal void AlertSongNameChanged(SongViewModel songViewModel, string oldName)
        {
            SongCollection.Remove(songViewModel, oldName);
            SongCollection.Add(songViewModel, songViewModel.SortName);
        }

        internal void DeleteSong(SongViewModel song)
        {
            foreach (PlaylistViewModel playlist in PlaylistCollection)
            {
                playlist.RemoveAllInstancesOfSong(song);
            }

            PlayQueue.RemoveAllInstancesOfSong(song);

            AlbumViewModel album = song.Album;
            ArtistViewModel artist = song.Artist;

            LibraryModel.Current.DeleteSong(song.SongId);

            RemoveAlbumIfNeeded(album);
            RemoveArtistIfNeeded(artist);
        }

        #endregion

        # region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}

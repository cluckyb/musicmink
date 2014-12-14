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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;

namespace MusicMink.ViewModels
{
    class LibraryViewModel : NotifyPropertyChangedUI
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
            LibraryModel.Current.Mixes.CollectionChanged += HandleMixCollectionChanged;

            LibraryModel.Current.AlbumCreated += HandleLibraryModelAlbumCreated;

            _playQueue = new PlayQueueViewModel(LibraryModel.Current.PlayQueue);

            NavigationManager.Current.PropertyChanged += HandleNavigationManagerPropertyChanged;

            perf.Trace("first pass load done");
        }

        void HandleNavigationManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case NavigationManager.Properties.IsHome:
                    GoHome.RaiseExecuteChanged();
                    break;
            }
        }

        public void PreInitalize()
        {
            PlayQueue.Initalize();
        }

        // Delayed constructor for once LibraryModel is loaded
        public Task InitalizeLibrary()
        {
            LibraryLoaded = false;
            MixesLoaded = false;

            return Task.Factory.StartNew(() =>
            {
                foreach (SongModel newSong in LibraryModel.Current.AllSongs)
                {
                    LookupSong(newSong);
                }

                foreach (PlaylistModel newPlaylist in LibraryModel.Current.Playlists)
                {
                    LookupPlaylist(newPlaylist);
                }

                foreach (MixModel newMix in LibraryModel.Current.Mixes)
                {
                    LookupMix(newMix);
                }

                LibraryLoaded = true;

                NotifyPropertyChanged(Properties.IsEmpty);

                foreach (MixViewModel newMix in MixCollection)
                {
                    newMix.Initalize();
                }

                MixesLoaded = true;
            });            
        }

        #region Properties

        public static class Properties
        {
            public const string LibraryLoaded = "LibraryLoaded";
            public const string MixesLoaded = "MixesLoaded";
            public const string IsEmpty = "IsEmpty";
        }

        private bool _libraryLoaded = false;
        public bool LibraryLoaded
        {
            get
            {
                return _libraryLoaded;
            }
            set
            {
                if (_libraryLoaded != value)
                {
                    _libraryLoaded = value;
                    NotifyPropertyChanged(Properties.LibraryLoaded);
                    AddNewPlaylist.RaiseExecuteChanged();
                }
            }
        }

        private bool _mixesLoaded = false;
        public bool MixesLoaded
        {
            get
            {
                return _mixesLoaded;
            }
            set
            {
                if (_mixesLoaded != value)
                {
                    _mixesLoaded = value;
                    NotifyPropertyChanged(Properties.MixesLoaded);
                    AddNewMix.RaiseExecuteChanged();
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                return FlatSongCollection.Count == 0;
            }
        }

        #endregion

        #region Event Handlers

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

        private void HandleMixCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (MixModel model in e.OldItems)
                {
                    MixCollection.Remove(MixLookupMap[model.MixId]);
                    MixLookupMap.Remove(model.MixId);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (MixModel model in e.NewItems)
                {
                    LookupMix(model);
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                MixCollection.Clear();
                MixLookupMap.Clear();
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

            if (LibraryLoaded)
            {
                NotifyPropertyChanged(Properties.IsEmpty);
                ShuffleAllSongs.RaiseExecuteChanged();
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
            return LibraryLoaded;
        }


        private RelayCommand _addNewMix;
        public RelayCommand AddNewMix
        {
            get
            {
                if (_addNewMix == null) _addNewMix = new RelayCommand(CanExecuteAddNewMix, ExecuteAddNewMix);

                return _addNewMix;
            }
        }

        private async void ExecuteAddNewMix(object parameter)
        {
            AddMix addMixDialog = new AddMix();

            await addMixDialog.ShowAsync();
        }

        private bool CanExecuteAddNewMix(object parameter)
        {
            return MixesLoaded;
        }

        private RelayCommand _navigate;
        public RelayCommand Navigate
        {
            get
            {
                if (_navigate == null) _navigate = new RelayCommand(CanExecuteNavigate, ExecuteNavigate);

                return _navigate;
            }
        }

        private void ExecuteNavigate(object parameter)
        {
            string parameterAsString = DebugHelper.CastAndAssert<string>(parameter);
            NavigationLocation target = NavigationLocation.Home;

            DebugHelper.Assert(new CallerInfo(), Enum.TryParse<NavigationLocation>(parameterAsString, out target), "Couldn't find location named {0}", parameterAsString);

            NavigationManager.Current.Navigate(target);
        }

        private bool CanExecuteNavigate(object parameter)
        {
            return true;
        }

        private RelayCommand _goHome;
        public RelayCommand GoHome
        {
            get
            {
                if (_goHome == null) _goHome = new RelayCommand(CanExecuteGoHome, ExecuteGoHome);

                return _goHome;
            }
        }

        private void ExecuteGoHome(object parameter)
        {
            NavigationManager.Current.GoHome();
        }

        private bool CanExecuteGoHome(object parameter)
        {
            return NavigationManager.Current.IsHome;
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
            int limit = int.Parse(DebugHelper.CastAndAssert<string>(parameter));

            LibraryViewModel.Current.PlayQueue.ShuffleSongList(FlatSongCollection, true, limit);
        }

        private bool CanExecuteShuffleAllSongs(object parameter)
        {
            int limit = int.Parse(DebugHelper.CastAndAssert<string>(parameter));

            return FlatSongCollection.Count > limit;
        }

        #endregion

        #region Collections

        private Dictionary<int, ArtistViewModel> ArtistLookupMap = new Dictionary<int, ArtistViewModel>();
        private Dictionary<int, AlbumViewModel> AlbumLookupMap = new Dictionary<int, AlbumViewModel>();
        private Dictionary<int, SongViewModel> SongLookupMap = new Dictionary<int, SongViewModel>();
        private Dictionary<int, PlaylistViewModel> PlaylistLookupMap = new Dictionary<int, PlaylistViewModel>();
        private Dictionary<int, MixViewModel> MixLookupMap = new Dictionary<int, MixViewModel>();

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

        private ObservableCollection<MixViewModel> _mixCollection = new ObservableCollection<MixViewModel>();
        public ObservableCollection<MixViewModel> MixCollection
        {
            get
            {
                return _mixCollection;
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

        public ArtistViewModel LookupArtistByName(string name, bool createIfNotFound = true)
        {
            return LookupArtist(LibraryModel.Current.LookupArtistByName(name, createIfNotFound));
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
                // If not, see if its in the database
                return LookupSong(LibraryModel.Current.LookupSongById(songId));
            }
        }

        internal SongViewModel LookupSongByName(string TrackName, string ArtistName)
        {
            ArtistViewModel artist = LookupArtistByName(ArtistName, false);

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

                if (LibraryLoaded)
                {
                    NotifyPropertyChanged(Properties.IsEmpty);
                }

                return newSongViewModel;
            }
        }

        internal bool AddSong(StorageProviderSong song, bool resetSongData)
        {
            SongModel songFromSource = LibraryModel.Current.GetSongFromSource(StorageProviderSourceToSongOriginSource(song.Origin), song.Source);
            if (songFromSource != null)
            {
                if (resetSongData)
                {
                    SongViewModel songViewModel = LookupSong(songFromSource);

                    songViewModel.Name = song.Name;

                    songViewModel.ArtistName = song.Artist;

                    string newAlbumName = song.Album;
                    string newAlbumAristName = song.AlbumArtist;

                    if (newAlbumName != songViewModel.Album.Name || newAlbumAristName != songViewModel.Album.ArtistName)
                    {
                        ArtistViewModel albumArtistViewModel = LibraryViewModel.Current.LookupArtistByName(newAlbumAristName);
                        AlbumViewModel newAlbumViewModel = LibraryViewModel.Current.LookupAlbumByName(newAlbumName, albumArtistViewModel.ArtistId);

                        songViewModel.UpdateAlbum(newAlbumViewModel);
                    }

                    songViewModel.TrackNumber = song.TrackNumber;
                }
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


        public MixViewModel LookupMixById(int mixId)
        {
            // check the cache, its probably there
            if (MixLookupMap.ContainsKey(mixId))
            {
                return MixLookupMap[mixId];
            }
            else
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Couldn't find mix {0} in cache", mixId);

                return null;
            }
        }

        public MixViewModel LookupMix(MixModel mix)
        {
            if (MixLookupMap.ContainsKey(mix.MixId))
            {
                return MixLookupMap[mix.MixId];
            }
            else
            {
                MixViewModel newViewModel = new MixViewModel(mix);

                MixLookupMap.Add(newViewModel.MixId, newViewModel);
                MixCollection.Add(newViewModel);
                return newViewModel;
            }
        }

        #endregion

        #region Methods

        internal void AddPlaylist(string playlistName)
        {
            LibraryModel.Current.AddPlaylist(playlistName);
        }

        internal void AddMix(string mixName)
        {
            LibraryModel.Current.AddMix(mixName);
        }

        internal void DeletePlaylist(PlaylistViewModel playlist)
        {
            playlist.IsBeingDeleted = true;

            LibraryModel.Current.DeletePlaylist(playlist.PlaylistId);
        }

        internal void DeleteMix(MixViewModel mix)
        {
            mix.IsBeingDeleted = true;

            LibraryModel.Current.DeleteMix(mix.MixId);
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

        internal void AlertSongChanged(SongViewModel songViewModel, string propertyName)
        {
            foreach (MixViewModel mix in MixCollection)
            {
                mix.OnSongPropertyChanged(songViewModel, propertyName);
            }
        }

        #endregion
    }
}

using MusicMink.Collections;
using MusicMink.Common;
using MusicMink.Dialogs;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Helpers;
using MusicMinkAppLayer.Models;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace MusicMink.ViewModels
{
    public class AlbumViewModel : BaseViewModel<AlbumModel>, IComparable
    {
        public static class Properties
        {
            public const string AlbumId = "AlbumId";

            public const string ArtistName = "ArtistName";
            public const string AlbumArt = "AlbumArt";
            public const string AlbumArtSource = "AlbumArtSource";
            public const string Artist = "Artist";
            public const string HasAlbumArt = "HasAlbumArt";
            public const string Length = "Length";
            public const string LengthInfo = "LengthInfo";
            public const string Name = "Name";
            public const string Songs = "SongCount";
            public const string SongCount = "SongCount";
            public const string SortName = "SortName";

            public const string IsBeingDeleted = "IsBeingDeleted";
        }

        public AlbumViewModel(AlbumModel album, ArtistViewModel artistViewModel)
            : base(album)
        {
            _artist = artistViewModel;
            _artist.Albums.Add(this);

            Artist.PropertyChanged += HandleArtistPropertyChanged;
            Songs.CollectionChanged += HandleSongCollectionChanged;
            album.PropertyChanged += HandleAlbumModelPropertyChanged;
        }

        #region Event Handlers

        void HandleArtistPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case ArtistViewModel.Properties.Name:
                    NotifyPropertyChanged(Properties.ArtistName);
                    break;
            }
        }

        private void HandleAlbumModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case AlbumModel.Properties.AlbumArt:
                    _albumArt = null;
                    NotifyPropertyChanged(Properties.AlbumArtSource);
                    NotifyPropertyChanged(Properties.HasAlbumArt);
                    UpdateAlbumArt();
                    break;
                case AlbumModel.Properties.ArtistId:
                    NotifyPropertyChanged(Properties.Artist);
                    NotifyPropertyChanged(Properties.ArtistName);
                    break;
                case AlbumModel.Properties.AlbumId:
                    DebugHelper.Alert(new CallerInfo(), "AlbumId Probably Shouldn't Change...");
                    NotifyPropertyChanged(Properties.AlbumId);
                    break;
                case AlbumModel.Properties.Name:
                    _sortName = null;
                    NotifyPropertyChanged(Properties.SortName);
                    NotifyPropertyChanged(Properties.Name);
                    break;
            }
        }

        private void HandleSongCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(Properties.SongCount);
            ResetLength();
        }

        #endregion

        #region Properties

        public int AlbumId
        {
            get
            {
                return rootModel.AlbumId;
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

        public string ArtistName
        {
            get
            {
                return Artist.Name;
            }
            set
            {
                Artist.Albums.Remove(this);
                ArtistViewModel oldArtist = Artist;

                _artist = LibraryViewModel.Current.LookupArtistByName(value);
                rootModel.ArtistId = Artist.ArtistId;

                Artist.Albums.Add(this);
                LibraryViewModel.Current.RemoveArtistIfNeeded(oldArtist);

                NotifyPropertyChanged(Properties.Artist);
                NotifyPropertyChanged(Properties.ArtistName);
            }
        }

        private Uri _albumArt;
        public Uri AlbumArt
        {
            get
            {
                if (HasAlbumArt && _albumArt == null)
                {
                    UpdateAlbumArt();
                }

                return _albumArt;
            }
        }

        public async Task<Uri> GetAlbumArt()
        {
            await UpdateAlbumArt();

            return AlbumArt;
        }

        private async Task UpdateAlbumArt()
        {
            if (!HasAlbumArt)
            {
                _albumArt = null;
                return;
            }

            IStorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(AlbumArtSource);

            if (file == null)
            {
                _albumArt = null;
                return;
            }

            _albumArt = new Uri(file.Path);

            NotifyPropertyChanged(Properties.AlbumArt);
        }


        public string AlbumArtSource
        {
            get
            {
                return rootModel.AlbumArt;
            }
        }

        public bool HasAlbumArt
        {
            get
            {
                return !string.IsNullOrEmpty(rootModel.AlbumArt);
            }
        }

        private ArtistViewModel _artist;
        public ArtistViewModel Artist
        {
            get
            {
                return _artist;
            }
        }

        private TimeSpan _length = TimeSpan.MinValue;
        public TimeSpan Length
        {
            get
            {
                if (_length == TimeSpan.MinValue)
                {
                    _length = TimeSpan.Zero;
                    foreach (SongViewModel song in Songs)
                    {
                        _length += song.Duration;
                    }
                }

                return _length;
            }
        }

        public string LengthInfo
        {
            get
            {
                return Strings.HandlePlural(SongCount, Strings.GetResource("FormatSongsPlural"), Strings.GetResource("FormatSongsSingular")) +
                    " (" + Strings.HandlePlural((int)Length.TotalMinutes, Strings.GetResource("FormatMinutesPlural"), Strings.GetResource("FormatMinutesSingular")) + ")";
            }
        }

        public string Name
        {
            get
            {
                if (rootModel.Name == string.Empty) return Strings.GetResource("UnknownAlbumString");
                return rootModel.Name;
            }
            set
            {
                if (rootModel.Name != value)
                {
                    string oldName = SortName;
                    rootModel.Name = value;

                    LibraryViewModel.Current.AlertAlbumNameChanged(this, oldName);
                }
            }
        }

        private SortedList<SongViewModel> _songs = new SortedList<SongViewModel>(new SongSortGenericOrder(SongViewModel.Properties.TrackNumber, true));
        public SortedList<SongViewModel> Songs
        {
            get
            {
                return _songs;
            }
        }

        public int SongCount
        {
            get
            {
                return Songs.Count;
            }
        }

        private string _sortName;
        public string SortName
        {
            get
            {
                if (_sortName == null)
                {
                    if (Name.Length <= 4) _sortName = Name;
                    else
                    {
                        if (Name.Substring(0, 4).ToLowerInvariant() == Strings.GetResource("TitleStartStripMatch"))
                        {
                            _sortName = Name.Substring(4);
                        }
                        else
                        {
                            _sortName = Name;
                        }
                    }
                }

                return _sortName;
            }
        }

        #endregion

        #region Methods

        public void ResetLength()
        {
            _length = TimeSpan.MinValue;
            NotifyPropertyChanged(Properties.Length);
            NotifyPropertyChanged(Properties.LengthInfo);
        }

        internal async Task SetArtToLastFM(bool clobber)
        {
            if (HasAlbumArt && !clobber) return;

            string LastFMArt = await LastFMManager.Current.GetAlbumArt(ArtistName, Name);

            if (!string.IsNullOrEmpty(LastFMArt))
            {
                UpdateArt(LastFMArt);
            }
        }

        internal string GetNewArtPath()
        {
            return "AlbumArt_" + AlbumId + ".img";
        }

        internal async void UpdateArt(string sourcePath)
        {
            if (!string.IsNullOrEmpty(sourcePath))
            {
                string albumArtPath = GetNewArtPath();

                if (await DownloadManager.Current.DownloadFile(new Uri(sourcePath), albumArtPath))
                {
                    rootModel.AlbumArt = albumArtPath;
                }
            }
        }

        internal async void UpdateArt(IStorageFile storageFile)
        {
            if (storageFile != null)
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                string albumArtPath = GetNewArtPath();

                try
                {
                    using (Stream downloadStream = await storageFile.OpenStreamForReadAsync())
                    {
                        using (var fileStream = await localFolder.OpenStreamForWriteAsync(albumArtPath, CreationCollisionOption.ReplaceExisting))
                        {
                            int chunkSize = 4096;
                            byte[] bytes = new byte[chunkSize];
                            int byteCount;

                            while ((byteCount = downloadStream.Read(bytes, 0, chunkSize)) > 0)
                            {
                                fileStream.Write(bytes, 0, byteCount);
                            }
                        }
                    }

                    rootModel.AlbumArt = albumArtPath;
                }
                catch (Exception e)
                {
                    Logger.Current.Log(new CallerInfo(), LogLevel.Error, "Exception in File Copy {0}", e.Message);
                }
            }
        }

        #endregion

        #region Commands

        private RelayCommand _playAlbum;
        public RelayCommand PlayAlbum
        {
            get
            {
                if (_playAlbum == null) _playAlbum = new RelayCommand(CanExecutePlayAlbum, ExecutePlayAlbum);

                return _playAlbum;
            }
        }

        private void ExecutePlayAlbum(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.PlaySongList(Songs, true);
        }

        private bool CanExecutePlayAlbum(object parameter)
        {
            return true;
        }


        private RelayCommand _queueAlbum;
        public RelayCommand QueueAlbum
        {
            get
            {
                if (_queueAlbum == null) _queueAlbum = new RelayCommand(CanExecuteQueueAlbum, ExecuteQueueAlbum);

                return _queueAlbum;
            }
        }

        private void ExecuteQueueAlbum(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.PlaySongList(Songs, false);
        }

        private bool CanExecuteQueueAlbum(object parameter)
        {
            return true;
        }


        private RelayCommand _shuffleAlbum;
        public RelayCommand ShuffleAlbum
        {
            get
            {
                if (_shuffleAlbum == null) _shuffleAlbum = new RelayCommand(CanExecuteShuffleAlbum, ExecuteShuffleAlbum);

                return _shuffleAlbum;
            }
        }

        private void ExecuteShuffleAlbum(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.ShuffleSongList(Songs, true);
        }

        private bool CanExecuteShuffleAlbum(object parameter)
        {
            return true;
        }


        private RelayCommand _editAlbum;
        public RelayCommand EditAlbum
        {
            get
            {
                if (_editAlbum == null) _editAlbum = new RelayCommand(CanExecuteEditAlbum, ExecuteEditAlbum);

                return _editAlbum;
            }
        }

        private async void ExecuteEditAlbum(object parameter)
        {
            EditAlbum editAlbumDialog = new EditAlbum(this);

            await editAlbumDialog.ShowAsync();
        }

        private bool CanExecuteEditAlbum(object parameter)
        {
            return true;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            AlbumViewModel otherAlbum = obj as AlbumViewModel;
            if (otherAlbum != null)
            {
                if (otherAlbum.Name == this.Name)
                {
                    return this.Artist.Name.CompareTo(otherAlbum.Artist.Name);
                }
                return this.Name.CompareTo(otherAlbum.Name);
            }
            else
            {
                throw new ArgumentException("Object is not a albumViewModel");
            }
        }

        #endregion

    }
}
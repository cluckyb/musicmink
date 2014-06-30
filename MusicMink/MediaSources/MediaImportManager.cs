using MusicMink.Common;
using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MusicMink.MediaSources
{
    class ImportStatsContinuationInfo : ContinuationInfo
    {
        public ImportStatsContinuationInfo()
        {
        }
    }

    public enum ActiveSyncSourceType
    {
        None,
        LocalLibrary
    }

    public class MediaImportManager : INotifyPropertyChanged
    {
        public static class Properties
        {
            public const string SongsFound = "SongsFound";
            public const string SongsSkipped = "SongsSkipped";
            public const string ActiveSyncSource = "ActiveSyncSource";

            public const string IsNoSyncInProgress = "IsNoSyncInProgress";
            public const string IsLocalSyncInProgress = "IsLocalSyncInProgress";

            public const string ArtSyncAlbumsLeft = "ArtSyncAlbumsLeft";
            public const string IsArtSyncInProgress = "IsArtSyncInProgress";

            public const string StatImportSongsFound = "StatImportSongsFound";
            public const string StatImportSongsSkipped = "StatImportSongsSkipped";
        }

        private LocalMusicLibraryStorageProvider localLibraryStorageProvider;

        private static MediaImportManager _current;
        public static MediaImportManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new MediaImportManager();
                }

                return _current;
            }
        }

        private MediaImportManager()
        {
            localLibraryStorageProvider = new LocalMusicLibraryStorageProvider();
            localLibraryStorageProvider.TrackScanned += HandleTrackScanned;

            this.PropertyChanged += HandleMediaImportManagerPropertyChanged;
        }

        #region EventHandlers

        void HandleMediaImportManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Properties.ActiveSyncSource:
                    ScanLocalLibrary.RaiseExecuteChanged();
                    CancelScanLocalLibrary.RaiseExecuteChanged();
                    NotifyPropertyChanged(Properties.IsLocalSyncInProgress);
                    NotifyPropertyChanged(Properties.IsNoSyncInProgress);
                    break;
                case Properties.IsArtSyncInProgress:
                    ScanLastFMForArt.RaiseExecuteChanged();
                    break;
            }
        }

        void HandleTrackScanned(object sender, TrackScannedEventArgs e)
        {
            if (LibraryViewModel.Current.AddSong(e.ScannedTrack, e.ScannedTrack.IsDifferent))
            {
                SongsFound++;
            }
            else
            {
                SongsSkipped++; 
            }
        }

        #endregion

        #region Properties

        private int _artSyncAlbumsLeft;
        public int ArtSyncAlbumsLeft
        {
            get
            {
                return _artSyncAlbumsLeft;
            }
            set
            {
                if (_artSyncAlbumsLeft != value)
                {
                    _artSyncAlbumsLeft = value;
                    NotifyPropertyChanged(Properties.ArtSyncAlbumsLeft);
                }
            }
        }


        private bool _isArtSyncInProgress = false;
        public bool IsArtSyncInProgress
        {
            get
            {
                return _isArtSyncInProgress;
            }
            set
            {
                if (_isArtSyncInProgress != value)
                {
                    _isArtSyncInProgress = value;
                    NotifyPropertyChanged(Properties.IsArtSyncInProgress);
                }
            }
        }

        private int _songsFound;
        public int SongsFound
        {
            get
            {
                return _songsFound;
            }
            set
            {
                if (_songsFound != value)
                {
                    _songsFound = value;
                    NotifyPropertyChanged(Properties.SongsFound);
                }
            }
        }

        private int _songsSkipped;
        public int SongsSkipped
        {
            get
            {
                return _songsSkipped;
            }
            set
            {
                if (_songsSkipped != value)
                {
                    _songsSkipped = value;
                    NotifyPropertyChanged(Properties.SongsSkipped);
                }
            }
        }

        private int _statImportSongsFound;
        public int StatImportSongsFound
        {
            get
            {
                return _statImportSongsFound;
            }
            set
            {
                if (_statImportSongsFound != value)
                {
                    _statImportSongsFound = value;
                    NotifyPropertyChanged(Properties.StatImportSongsFound);
                }
            }
        }

        private int _statImportSongsSkipped;
        public int StatImportSongsSkipped
        {
            get
            {
                return _statImportSongsSkipped;
            }
            set
            {
                if (_statImportSongsSkipped != value)
                {
                    _statImportSongsSkipped = value;
                    NotifyPropertyChanged(Properties.StatImportSongsSkipped);
                }
            }
        }

        private ActiveSyncSourceType _activeSyncSource;
        public ActiveSyncSourceType ActiveSyncSource
        {
            get
            {
                return _activeSyncSource;
            }
            set
            {
                if (_activeSyncSource != value)
                {
                    _activeSyncSource = value;
                    NotifyPropertyChanged(Properties.ActiveSyncSource);
                }
            }
        }

        public bool IsNoSyncInProgress
        {
            get
            {
                return ActiveSyncSource == ActiveSyncSourceType.None;
            }
        }

        public bool IsLocalSyncInProgress
        {
            get
            {
                return ActiveSyncSource == ActiveSyncSourceType.LocalLibrary;
            }
        }

        #endregion

        #region Commands

        private RelayCommand _importStatFile;
        public RelayCommand ImportStatFile
        {
            get
            {
                if (_importStatFile == null) _importStatFile = new RelayCommand(CanExecuteImportStatFile, ExecuteImportStatFile);

                return _importStatFile;
            }
        }

        private void ExecuteImportStatFile(object parameter)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".txt");

            NavigationManager.Current.ContinuationInfo = new ImportStatsContinuationInfo();

            openPicker.PickSingleFileAndContinue();
        }

        internal async void HandleFilePickerLaunch(FileOpenPickerContinuationEventArgs filePickerOpenArgs)
        {
            await HandleFilePickerLaunchInternal(filePickerOpenArgs);            
        }

        private bool _canExecuteImportStatFile = true;
        private bool CanExecuteImportStatFile(object parameter)
        {
            return _canExecuteImportStatFile;
        }

        private RelayCommand _scanLastFMForArt;
        public RelayCommand ScanLastFMForArt
        {
            get
            {
                if (_scanLastFMForArt == null) _scanLastFMForArt = new RelayCommand(CanExecuteScanLastFMForArt, ExecuteScanLastFMForArt);

                return _scanLastFMForArt;
            }
        }

        private async void ExecuteScanLastFMForArt(object parameter)
        {
            await ScanLastmForArtInternal();
        }

        private bool CanExecuteScanLastFMForArt(object parameter)
        {
            return !IsArtSyncInProgress;
        }


        private RelayCommand _scanLocalLibrary;
        public RelayCommand ScanLocalLibrary
        {
            get
            {
                if (_scanLocalLibrary == null) _scanLocalLibrary = new RelayCommand(CanExecuteScanLocalLibrary, ExecuteScanLocalLibrary);

                return _scanLocalLibrary;
            }
        }

        private void ExecuteScanLocalLibrary(object parameter)
        {
            ScanLocalLibraryInternal();
        }

        private bool CanExecuteScanLocalLibrary(object parameter)
        {
            return ActiveSyncSource == ActiveSyncSourceType.None;
        }

        private RelayCommand _cancelScanLocalLibrary;
        public RelayCommand CancelScanLocalLibrary
        {
            get
            {
                if (_cancelScanLocalLibrary == null) _cancelScanLocalLibrary = new RelayCommand(CanExecuteCancelScanLocalLibrary, ExecuteCancelScanLocalLibrary);

                return _cancelScanLocalLibrary;
            }
        }

        private void ExecuteCancelScanLocalLibrary(object parameter)
        {
            CancelLocalLibraryScanInternal();
        }

        private bool CanExecuteCancelScanLocalLibrary(object parameter)
        {
            return ActiveSyncSource == ActiveSyncSourceType.LocalLibrary;
        }

        #endregion

        #region Helper Methods

        private async void ScanLocalLibraryInternal()
        {
            if (ActiveSyncSource != ActiveSyncSourceType.None) return;

            ActiveSyncSource = ActiveSyncSourceType.LocalLibrary;

            SongsFound = 0;
            SongsSkipped = 0;

            await localLibraryStorageProvider.SyncStorageSolution();

            ActiveSyncSource = ActiveSyncSourceType.None;
        }

        private void CancelLocalLibraryScanInternal()
        {
            DebugHelper.Assert(new CallerInfo(), ActiveSyncSource == ActiveSyncSourceType.LocalLibrary);

            localLibraryStorageProvider.Cancel();
        }

        private async Task ScanLastmForArtInternal()
        {
            if (IsArtSyncInProgress) return;

            IsArtSyncInProgress = true;

            foreach (var albumCollection in LibraryViewModel.Current.AlbumCollection.Root)
            {
                ArtSyncAlbumsLeft += albumCollection.Count;
            }

            foreach (var albumCollection in LibraryViewModel.Current.AlbumCollection.Root)
            {
                foreach (AlbumViewModel album in albumCollection)
                {
                    await album.SetArtToLastFM(false);
                    ArtSyncAlbumsLeft--;
                }
            }

            IsArtSyncInProgress = false;
        }

        private async Task HandleFilePickerLaunchInternal(FileOpenPickerContinuationEventArgs filePickerOpenArgs)
        {
            _canExecuteImportStatFile = false;
            ImportStatFile.RaiseExecuteChanged();

            StatImportSongsFound = 0;
            StatImportSongsSkipped = 0;

            DebugHelper.Assert(new CallerInfo(), filePickerOpenArgs.Files.Count == 1);

            IStorageFile pickedFile = filePickerOpenArgs.Files[0];

            using (Stream stream = await pickedFile.OpenStreamForReadAsync())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        string[] parts = line.Split(new char[] { '|' });

                        if (parts.Count() == 6)
                        {
                            string TrackName = parts[0];
                            string AlbumName = parts[1];
                            string ArtistName = parts[2];
                            string Rating = parts[3];
                            string PlayCount = parts[4];
                            string LastPlayed = parts[5];

                            SongViewModel song = LibraryViewModel.Current.LookupSongByName(TrackName, ArtistName);

                            if (song != null)
                            {
                                song.Rating = uint.Parse(Rating);

                                // TODO: let user toggle this
                                // song.PlayCount = uint.Parse(PlayCount);

                                DateTime realLastPlayed = new DateTime(long.Parse(LastPlayed));

                                if (song.LastPlayed < realLastPlayed)
                                {
                                    song.LastPlayed = realLastPlayed;
                                }

                                StatImportSongsFound++;
                            }
                            else
                            {
                                StatImportSongsSkipped++;
                            }
                        }
                    }
                }
            }

            _canExecuteImportStatFile = true;
            ImportStatFile.RaiseExecuteChanged();
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

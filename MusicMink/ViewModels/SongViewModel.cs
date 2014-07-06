using MusicMink.Common;
using MusicMink.Dialogs;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace MusicMink.ViewModels
{
    public class SongViewModel : BaseViewModel<SongModel>, IComparable
    {
        public static class Properties
        {
            public const string SongId = "SongId";

            public const string Album = "Album";
            public const string AlbumName = "AlbumName";

            public const string AlbumArtistName = "AlbumArtistName";

            public const string Artist = "Artist";
            public const string ArtistName = "ArtistName";

            public const string Duration = "Duration";
            public const string DurationText = "DurationText";
            public const string DurationSeconds = "DurationSeconds";
            public const string ExtraInfoString = "ExtraInfoString";
            public const string LastPlayed = "LastPlayed";
            public const string Name = "Name";
            public const string PlayCount = "PlayCount";
            public const string Rating = "Rating";
            public const string SortName = "SortName";
            public const string TrackNumber = "TrackNumber";
        }

        public SongViewModel(SongModel song, ArtistViewModel artistViewModel, AlbumViewModel albumViewModel) : base(song)
        {
            Artist = artistViewModel;
            Artist.Songs.Add(this);

            Album = albumViewModel;
            Album.Songs.Add(this);

            rootModel.PropertyChanged += HandleSongModelPropertyChanged;
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

        void HandleAlbumPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case AlbumViewModel.Properties.Name:
                    NotifyPropertyChanged(Properties.AlbumName);
                    break;
                case AlbumViewModel.Properties.ArtistName:
                    NotifyPropertyChanged(Properties.AlbumArtistName);
                    break;
            }
        }

        void HandleSongModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case SongModel.Properties.AlbumId:
                    NotifyPropertyChanged(Properties.Album);
                    NotifyPropertyChanged(Properties.AlbumName);
                    NotifyPropertyChanged(Properties.AlbumArtistName);
                    break;
                case SongModel.Properties.ArtistId:
                    NotifyPropertyChanged(Properties.Artist);
                    NotifyPropertyChanged(Properties.ArtistName);
                    break;
                case SongModel.Properties.Duration:
                    Album.ResetLength();
                    NotifyPropertyChanged(Properties.Duration);
                    NotifyPropertyChanged(Properties.DurationText);
                    NotifyPropertyChanged(Properties.DurationSeconds);
                    break;
                case SongModel.Properties.SongId:
                    DebugHelper.Alert(new CallerInfo(), "SongId Shouldn't Change");
                    NotifyPropertyChanged(Properties.SongId);
                    break;
                case SongModel.Properties.Name:
                    _sortName = null;
                    NotifyPropertyChanged(Properties.SortName);
                    NotifyPropertyChanged(Properties.Name);
                    break;
                case SongModel.Properties.PlayCount:
                    NotifyPropertyChanged(Properties.PlayCount);
                    NotifyPropertyChanged(Properties.ExtraInfoString);
                    break;
                case SongModel.Properties.Rating:
                    NotifyPropertyChanged(Properties.Rating);
                    break;
                case SongModel.Properties.TrackNumber:
                    NotifyPropertyChanged(Properties.TrackNumber);
                    break;
                case SongModel.Properties.LastPlayed:
                    _lastPlayed = null;
                    NotifyPropertyChanged(Properties.LastPlayed);
                    NotifyPropertyChanged(Properties.ExtraInfoString);
                    break;
            }

            LibraryViewModel.Current.AlertSongChanged(this, e.PropertyName);
        }

        #endregion

        #region Properties

        public int SongId
        {
            get
            {
                return rootModel.SongId;
            }
        }

        private AlbumViewModel _album;
        public AlbumViewModel Album
        {
            get
            {
                return _album;
            }
            private set
            {
                if (_album != value)
                {
                    if (_album != null)
                    {
                        _album.PropertyChanged -= HandleAlbumPropertyChanged;
                    }
                    _album = value;
                    if (_album != null)
                    {
                        _album.PropertyChanged += HandleAlbumPropertyChanged;
                    }
                }
            }
        }

        public string AlbumName
        {
            get
            {
                return Album.Name;
            }
        }

        public void UpdateAlbum(AlbumViewModel newAlbum)
        {
            // Set the root AlbumId so we can remove the old Album
            rootModel.AlbumId = newAlbum.AlbumId;

            Album.Songs.Remove(this);
            LibraryViewModel.Current.RemoveAlbumIfNeeded(Album);

            // Then set the new album and fill it out
            Album = newAlbum;

            Album.Songs.Add(this);

            NotifyPropertyChanged(Properties.Album);
        }

        private ArtistViewModel _artist;
        public ArtistViewModel Artist
        {
            get
            {
                return _artist;
            
            }
            set
            {
                if (_artist != value)
                {
                    if (_artist != null)
                    {
                        _artist.PropertyChanged -= HandleArtistPropertyChanged;
                    }
                    _artist = value;
                    if (_artist != null)
                    {
                        _artist.PropertyChanged += HandleArtistPropertyChanged;
                    }
                }
            }
        }

        public string AlbumArtistName
        {
            get
            {
                return Album.ArtistName;
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
                if (value != Artist.Name)
                {
                    Artist.Songs.Remove(this);
                    ArtistViewModel oldArtist = Artist;

                    Artist = LibraryViewModel.Current.LookupArtistByName(value);

                    rootModel.ArtistId = Artist.ArtistId;

                    Artist.Songs.Add(this);

                    LibraryViewModel.Current.RemoveArtistIfNeeded(oldArtist);
                }
            }
        }

        private TimeSpan? _duration;
        public TimeSpan Duration
        {
            get
            {
                if (!_duration.HasValue)
                {
                    _duration = TimeSpan.FromTicks(rootModel.Duration);
                }

                return _duration.Value;
            }
        }

        public long DurationSeconds
        {
            get
            {
                return (int) Duration.TotalSeconds;
            }
        }

        public string DurationText
        {
            get
            {
                return Duration.ToString(@"%m\:ss");
            }
        }

        public string ExtraInfoString
        {
            get
            {
                if (LastPlayed != DateTime.MinValue)
                {
                    return String.Format(Strings.GetResource("SongExtraInfoDate"),
                        Strings.HandlePlural(PlayCount, Strings.GetResource("SongExtraInfoPlays"), Strings.GetResource("SongExtraInfoPlay")),
                        LastPlayed.ToLocalTime().ToString());
                }
                else
                {
                    return Strings.HandlePlural(PlayCount, Strings.GetResource("SongExtraInfoPlays"), Strings.GetResource("SongExtraInfoPlay"));
                }
            }
        }

        public string Name
        {
            get
            {
                if (rootModel.Name == string.Empty) return Strings.GetResource("UnknownSongString");
                return rootModel.Name;
            }
            set
            {
                if (rootModel.Name != value)
                {
                    string oldName = SortName;
                    rootModel.Name = value;
                    LibraryViewModel.Current.AlertSongNameChanged(this, oldName);
                }
            }
        }

        private DateTime? _lastPlayed;
        public DateTime LastPlayed
        {
            get
            {
                if (!_lastPlayed.HasValue)
                {
                    _lastPlayed = new DateTime(rootModel.LastPlayed);
                }

                return _lastPlayed.Value;
            }
            set
            {
                if (rootModel.LastPlayed != value.Ticks)
                {
                    rootModel.LastPlayed = value.Ticks;
                }
            }
        }

        public uint PlayCount
        {
            get
            {
                return rootModel.PlayCount;
            }
            set
            {
                if (rootModel.PlayCount != value)
                {
                    rootModel.PlayCount = value;
                }
            }
        }

        public uint Rating
        {
            get
            {
                return rootModel.Rating;
            }
            set
            {
                if (rootModel.Rating != value)
                {
                    rootModel.Rating = value;
                }
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

        public uint TrackNumber
        {
            get
            {
                return rootModel.TrackNumber;
            }
            set
            {
                if (rootModel.TrackNumber != value)
                {
                    // This will trigger resort
                    Album.Songs.Remove(this);
                    rootModel.TrackNumber = value;
                    Album.Songs.Add(this);
                }
            }
        }

        public string Source
        {
            get
            {
                return rootModel.Source;
            }
        }

        #endregion

        #region Commands

        private RelayCommand _playSong;
        public RelayCommand PlaySong
        {
            get
            {
                if (_playSong == null) _playSong = new RelayCommand(CanExecutePlaySong, ExecutePlaySong);

                return _playSong;
            }
        }

        private void ExecutePlaySong(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.PlaySong(this);
        }

        private bool CanExecutePlaySong(object parameter)
        {
            return true;
        }

        private RelayCommand _queueSong;
        public RelayCommand QueueSong
        {
            get
            {
                if (_queueSong == null) _queueSong = new RelayCommand(CanExecuteQueueSong, ExecuteQueueSong);

                return _queueSong;
            }
        }

        private void ExecuteQueueSong(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.QueueSong(this);
        }

        private bool CanExecuteQueueSong(object parameter)
        {
            return true;
        }


        private RelayCommand _addSongToPlaylist;
        public RelayCommand AddSongToPlaylist
        {
            get
            {
                if (_addSongToPlaylist == null) _addSongToPlaylist = new RelayCommand(CanExecuteAddSongToPlaylist, ExecuteAddSongToPlaylist);

                return _addSongToPlaylist;
            }
        }

        private async void ExecuteAddSongToPlaylist(object parameter)
        {
            AddToPlaylist addToPlaylistDialog = new AddToPlaylist(this);

            await addToPlaylistDialog.ShowAsync();
        }

        private bool CanExecuteAddSongToPlaylist(object parameter)
        {
            return true;
        }

        private RelayCommand _editSong;
        public RelayCommand EditSong
        {
            get
            {
                if (_editSong == null) _editSong = new RelayCommand(CanExecuteEditSong, ExecuteEditSong);

                return _editSong;
            }
        }

        private async void ExecuteEditSong(object parameter)
        {
            EditSong editSongDialog = new EditSong(this);

            await editSongDialog.ShowAsync();
        }

        private bool CanExecuteEditSong(object parameter)
        {
            return true;
        }
        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            SongViewModel otherSong = obj as SongViewModel;
            if (otherSong != null)
            {
                if (otherSong.SortName == this.SortName)
                {
                    return this.Album.CompareTo(otherSong.Album);
                }
                else
                {
                    return this.SortName.CompareTo(otherSong.SortName);
                }
            }
            else
            {
                throw new ArgumentException("Object is not a SongViewModel");
            }
        }

        #endregion
    }

    public class SongSortGenericOrder : Comparer<SongViewModel>
    {
        PropertyInfo sortInfo;
        bool invert;

        public SongSortGenericOrder(string sort, bool i)
        {
            sortInfo = typeof(SongViewModel).GetRuntimeProperty(sort);

            invert = i;
        }

        // Compares by Length, Height, and Width. 
        public override int Compare(SongViewModel x, SongViewModel y)
        {
            object t1 = sortInfo.GetValue(x);
            object t2 = sortInfo.GetValue(y);

            IComparable c1 = t1 as IComparable;
            IComparable c2 = t2 as IComparable;

            int compare = 0;

            if (c1 == null)
            {
                if (c2 == null) return 0;

                compare = (invert ? -c2.CompareTo(c1) : c2.CompareTo(c1));
            }

            compare = (invert ? c1.CompareTo(c2) : -c1.CompareTo(c2));

            if (compare == 0)
            {
                return x.SongId.CompareTo(y.SongId);
            }
            else
            {
                return compare;
            }
        }
    }
}

using MusicMink.Common;
using MusicMinkAppLayer.Models;
using System;
using System.ComponentModel;

namespace MusicMink.ViewModels
{
    public class PlayQueueEntryViewModel : BaseViewModel<PlayQueueEntryModel>
    {
        public static class Properties
        {
            public const string RowId = "RowId";
            public const string Song = "Song";

            public const string IsPlaying = "IsPlaying";

            public const string IsPlayed = "IsPlayed";
            public const string RemainingPosition = "RemainingPosition";

            public const string TotalPosition = "TotalPosition";
        }

        public PlayQueueEntryViewModel(PlayQueueEntryModel model)
            : base(model)
        {
            model.PropertyChanged += HandleModelPropertyChanged;

            LibraryViewModel.Current.PlayQueue.PropertyChanged += HandlePlayQueuePropertyChanged;
        }

        #region Event Handlers

        void HandlePlayQueuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case PlayQueueViewModel.Properties.CurrentTrackRowId:
                    NotifyPropertyChanged(Properties.IsPlaying);
                    NotifyPropertyChanged(Properties.IsPlayed);
                    break;
                case PlayQueueViewModel.Properties.CurrentTrackPosition:
                    NotifyPropertyChanged(Properties.RemainingPosition);
                    NotifyPropertyChanged(Properties.IsPlayed);
                    break;
            }
        }

        void HandleModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case PlayQueueEntryModel.Properties.RowId:
                    _song = null;
                    NotifyPropertyChanged(Properties.RowId);
                    NotifyPropertyChanged(Properties.Song);
                    NotifyPropertyChanged(Properties.RemainingPosition);
                    NotifyPropertyChanged(Properties.IsPlaying);
                    NotifyPropertyChanged(Properties.IsPlayed);
                    break;
            }
        }

        #endregion

        #region Properties

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

        public int RowId
        {
            get
            {
                return rootModel.RowId;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return (RowId == LibraryViewModel.Current.PlayQueue.CurrentTrackRowId);
            }
        }

        public bool IsPlayed
        {
            get
            {
                return !IsPlaying && (RemainingPosition == 0);
            }
        }

        private int _totalPosition = 0;
        public int TotalPosition
        {
            get
            {
                return _totalPosition;
            }
            set
            {
                if (_totalPosition != value)
                {
                    _totalPosition = value;
                    NotifyPropertyChanged(Properties.TotalPosition);
                    NotifyPropertyChanged(Properties.RemainingPosition);
                    NotifyPropertyChanged(Properties.IsPlayed);
                }
            }
        }

        public int RemainingPosition
        {
            get
            {
                return Math.Max(0, (TotalPosition - LibraryViewModel.Current.PlayQueue.CurrentTrackPosition));
            }
        }

        #endregion

        #region Commands

        private RelayCommand _removeFromQueue;
        public RelayCommand RemoveFromQueue
        {
            get
            {
                if (_removeFromQueue == null) _removeFromQueue = new RelayCommand(CanExecuteRemoveFromQueue, ExecuteRemoveFromQueue);

                return _removeFromQueue;
            }
        }

        public void ExecuteRemoveFromQueue(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.RemoveSong(this);
        }

        public bool CanExecuteRemoveFromQueue(object parameter)
        {
            return true;
        }


        private RelayCommand _playFromHere;
        public RelayCommand PlayFromHere
        {
            get
            {
                if (_playFromHere == null) _playFromHere = new RelayCommand(CanExecutePlayFromHere, ExecutePlayFromHere);

                return _playFromHere;
            }
        }

        public void ExecutePlayFromHere(object parameter)
        {
            LibraryViewModel.Current.PlayQueue.PlayFromSong(this);
        }

        public bool CanExecutePlayFromHere(object parameter)
        {
            return true;
        }


        #endregion
    }
}

using MusicMink.Collections;
using MusicMink.Common;
using MusicMink.Dialogs;
using MusicMink.ViewModels.MixEvaluators;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Models;
using MusicMinkAppLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Windows.ApplicationModel.Core;

namespace MusicMink.ViewModels
{
    public class MixViewModel : BaseViewModel<MixModel>
    {
        public static class Properties
        {
            public const string MixId = "MixId";

            public const string Name = "Name";
            public const string IsCircular = "IsCircular";
            public const string Limit = "Limit";
            public const string HasLimit = "HasLimit";
            public const string SortType = "SortType";
            public const string Length = "Length";
            public const string LengthInfo = "LengthInfo";
            public const string IsHidden = "IsHidden";

            public const string IsBeingDeleted = "IsBeingDeleted";
        }

        public List<MixViewModel> DependentMixes = new List<MixViewModel>();

        public IMixEvaluator RootEvaluator;

        public MixViewModel(MixModel mix)
            : base(mix)
        {
            mix.PropertyChanged += HandleMixModelPropertyChanged;

            RootEvaluator = MixEntryModelToMixEvaluator(mix.RootMixEntry);

            _currentSongs = new LimitedList<SongViewModel>(new SongSortGenericOrder(MixSortOrderToSongProperty(mix.SortType), MixSortOrderToIsAscending(mix.SortType)), mix.Limit, mix.HasLimit);

            ResetLength();
        }

        #region Event Handlers

        private void HandleFlatSongCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (SongViewModel song in e.OldItems)
                {
                    ReevaulateSong(song);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (SongViewModel song in e.NewItems)
                {
                    ReevaulateSong(song);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                CurrentSongs.Clear();

                if (!IsCircular)
                {
                    foreach (MixViewModel mixViewModel in DependentMixes)
                    {
                        mixViewModel.Reset();
                    }
                }
            }
        }

        private void HandleCurrentSongsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetLength();

            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (SongViewModel song in e.OldItems)
                {
                    foreach (MixViewModel mixViewModel in DependentMixes)
                    {
                        mixViewModel.ReevaulateSong(song);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (SongViewModel song in e.NewItems)
                {
                    foreach (MixViewModel mixViewModel in DependentMixes)
                    {
                        mixViewModel.ReevaulateSong(song);
                    }
                }
            }
        }

        void HandleMixModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case MixModel.Properties.HasLimit:
                    NotifyPropertyChanged(Properties.HasLimit);
                    break;
                case MixModel.Properties.IsHidden:
                    NotifyPropertyChanged(Properties.IsHidden);
                    break;
                case MixModel.Properties.Limit:
                    NotifyPropertyChanged(Properties.Limit);
                    break;
                case MixModel.Properties.Name:
                    NotifyPropertyChanged(Properties.Name);
                    break;
                case MixModel.Properties.SortType:
                    NotifyPropertyChanged(Properties.SortType);
                    break;
            }
        }

        #endregion

        #region Properties

        public int MixId
        {
            get
            {
                return rootModel.MixId;
            }
        }

        private bool _isCircular;
        public bool IsCircular
        {
            get
            {
                return _isCircular;
            }
            set
            {
                if (_isCircular != value)
                {
                    _isCircular = value;
                    NotifyPropertyChanged(Properties.IsCircular);
                }
            }
        }

        private LimitedList<SongViewModel> _currentSongs;
        public LimitedList<SongViewModel> CurrentSongs
        {
            get
            {
                return _currentSongs;
            }
        }

        public string Name
        {
            get
            {
                if (rootModel.Name == string.Empty) return Strings.GetResource("UnknownAlbumString");
                else return rootModel.Name;
            }
            set
            {
                if (rootModel.Name != value)
                {
                    rootModel.Name = value;
                }
            }
        }

        public uint Limit
        {
            get
            {
                return rootModel.Limit;
            }
            set
            {
                rootModel.Limit = value;
            }
        }

        public bool HasLimit
        {
            get
            {
                return rootModel.HasLimit;
            }
            set
            {
                rootModel.HasLimit = value;
            }
        }

        public MixSortOrder SortType
        {
            get
            {
                return rootModel.SortType;
            }
            set
            {
                rootModel.SortType = value;
            }
        }

        public bool IsHidden
        {
            get
            {
                return rootModel.IsHidden;
            }
            set
            {
                if (rootModel.IsHidden != value)
                {
                    rootModel.IsHidden = value;
                }
            }
        }

        // TODO: share this with other things that also have a length
        private TimeSpan _length = TimeSpan.MinValue;
        public TimeSpan Length
        {
            get
            {
                if (_length == TimeSpan.MinValue)
                {
                    _length = TimeSpan.Zero;
                    foreach (SongViewModel song in CurrentSongs)
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
                return Strings.HandlePlural(CurrentSongs.Count, Strings.GetResource("FormatSongsPlural"), Strings.GetResource("FormatSongsSingular")) +
                    " (" + Strings.FormatTimeSpanLong((int)Length.TotalMinutes) + ")";
            }
        }

        internal void ResetLength()
        {
            _length = TimeSpan.MinValue;
            NotifyPropertyChanged(Properties.Length);
            NotifyPropertyChanged(Properties.LengthInfo);
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

        #endregion

        #region MixEntryModelToMixEvaluator

        public IMixEvaluator MixEntryModelToMixEvaluator(MixEntryModel mixEntryModel)
        {
            if (mixEntryModel == null) return new NoneMixEvaluator();

            switch (mixEntryModel.Type & MixType.TYPE_MASK)
            {
                case MixType.NUMBER_TYPE:
                    return NumericMixEntryModelToMixEvaluator(mixEntryModel.Type, mixEntryModel.Input);
                case MixType.STRING_TYPE:
                    return StringMixEntryModelToMixEvaluator(mixEntryModel.Type, mixEntryModel.Input);
                case MixType.NESTED_TYPE:
                    return NestedMixEntryModelToMixEvaluator(mixEntryModel);
                case MixType.RANGE_TYPE:
                    return RangeMixEntryModelToMixEvaluator(mixEntryModel.Type, mixEntryModel.Input);
                case MixType.MEMBER_TYPE:
                    return MemberMixEntryModelToMixEvaluator(mixEntryModel.Type, mixEntryModel.Input);
                default:
                    DebugHelper.Assert(new CallerInfo(), mixEntryModel.Type == MixType.None, "Unexpected mix type: {0}", mixEntryModel.Type);
                    return new NoneMixEvaluator();
            }
        }

        public static IMixEvaluator NumericMixEntryModelToMixEvaluator(MixType type, string input)
        {
            PropertyInfo info = null;
            switch (type & MixType.SUBTYPE_MASK)
            {
                case MixType.RATING_SUBTYPE:
                    info = typeof(SongViewModel).GetRuntimeProperty(SongViewModel.Properties.Rating);
                    break;
                case MixType.LENGTH_SUBTYPE:
                    info = typeof(SongViewModel).GetRuntimeProperty(SongViewModel.Properties.DurationSeconds);
                    break;
                case MixType.PLAYCOUNT_SUBTYPE:
                    info = typeof(SongViewModel).GetRuntimeProperty(SongViewModel.Properties.PlayCount);
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected NUMBER_TYPE MixType {0}", type);
                    break;
            }

            NumericEvalType numericEvalType = NumericEvalType.Unknown;
            switch (type & MixType.VARIANT_MASK)
            {
                case MixType.NUMERIC_STRICTLYLESS_VARIANT:
                    numericEvalType = NumericEvalType.StrictLess;
                    break;
                case MixType.NUMERIC_LESS_VARIANT:
                    numericEvalType = NumericEvalType.Less;
                    break;
                case MixType.NUMERIC_EQUAL_VARIANT:
                    numericEvalType = NumericEvalType.Equal;
                    break;
                case MixType.NUMERIC_MORE_VARIANT:
                    numericEvalType = NumericEvalType.More;
                    break;
                case MixType.NUMERIC_STRICTLYMORE_VARIANT:
                    numericEvalType = NumericEvalType.StrictMore;
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected NUMBER_TYPE VARIANT_MASK MixType {0}", type);
                    break;
            }

            if (info != null)
            {
                Type t = info.PropertyType;
                
                IComparable inputAsType = DebugHelper.CastAndAssert<IComparable>(Utilities.GetDefault(t));
                
                try
                {
                    inputAsType = DebugHelper.CastAndAssert<IComparable>(Convert.ChangeType(input, t));
                }
                catch (FormatException)
                {
                    // If cast is bad, ignore and use default type
                }

                return new NumericMixEvaluator<IComparable>(info, inputAsType, numericEvalType, type);
            }

            return new NoneMixEvaluator();
        }

        public static IMixEvaluator StringMixEntryModelToMixEvaluator(MixType type, string input)
        {
            PropertyInfo info = null;
            switch (type & MixType.SUBTYPE_MASK)
            {
                case MixType.ALBUM_SUBTYPE:
                    info = typeof(SongViewModel).GetRuntimeProperty(SongViewModel.Properties.AlbumSortName);
                    break;
                case MixType.ALBUMARTIST_SUBTYPE:
                    info = typeof(SongViewModel).GetRuntimeProperty(SongViewModel.Properties.AlbumArtistSortName);
                    break;
                case MixType.ARTIST_SUBTYPE:
                    info = typeof(SongViewModel).GetRuntimeProperty(SongViewModel.Properties.AlbumSortName);
                    break;
                case MixType.TRACK_SUBTYPE:
                    info = typeof(SongViewModel).GetRuntimeProperty(SongViewModel.Properties.Name);
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected ALBUM_TYPE MixType {0}", type);
                    break;
            }

            StringEvalType stringEvalType = StringEvalType.Unknown;
            switch (type & MixType.VARIANT_MASK)
            {
                case MixType.STRING_CONTAINS_VARIANT:
                    stringEvalType = StringEvalType.SubString;
                    break;
                case MixType.STRING_ENDSWITH_VARIANT:
                    stringEvalType = StringEvalType.EndsWith;
                    break;
                case MixType.STRING_EQUAL_VARIANT:
                    stringEvalType = StringEvalType.Equal;
                    break;
                case MixType.STRING_STARTSWITH_VARIANT:
                    stringEvalType = StringEvalType.StartsWith;
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected ALBUM_TYPE VARIANT_MASK MixType {0}", type);
                    break;
            }

            if (info != null)
            {
                return new StringMixEvaluator(info, input, stringEvalType, type);
            }

            return new NoneMixEvaluator();
        }

        private IMixEvaluator NestedMixEntryModelToMixEvaluator(MixEntryModel mixEntryModel)
        {
            string[] parts = mixEntryModel.Input.Split(NestedMixEvaluator.SplitToken[0]);
            List<IMixEvaluator> mixes = new List<IMixEvaluator>();
            foreach (string s in parts)
            {
                mixes.Add(MixEntryModelToMixEvaluator(rootModel.LookupMixEntry(int.Parse(s))));
            }

            return NestedMixEntryModelToMixEvaluator(mixEntryModel.Type, mixes);
        }

        public static IMixEvaluator NestedMixEntryModelToMixEvaluator(MixType mixType, List<IMixEvaluator> mixes)
        {
            NestedEvalType nestedEvalType = NestedEvalType.Unknown;

            switch (mixType)
            {
                case MixType.And:
                    nestedEvalType = NestedEvalType.All;
                    break;
                case MixType.Or:
                    nestedEvalType = NestedEvalType.Any;
                    break;
                case MixType.Not:
                    nestedEvalType = NestedEvalType.None;
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected NESTED_TYPE MixType {0}", mixType);
                    break;
            }

            return new NestedMixEvaluator(mixes, nestedEvalType, mixType);
        }

        public static IMixEvaluator MemberMixEntryModelToMixEvaluator(MixType mixType, string input)
        {
            MemberEvalType memberEvalType = MemberEvalType.Unknown;
            switch (mixType & MixType.SUBTYPE_MASK)
            {
                case MixType.MIXMEMBER_SUBTYPE:
                    memberEvalType = MemberEvalType.Mix;
                    break;
                case MixType.PLAYLISTMEMBER_SUBTYPE:
                    memberEvalType = MemberEvalType.Playlist;
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected MEMBER_TYPE MixType {0}", mixType);
                    break;
            }

            return new MemberMixEvaluator(int.Parse(input), memberEvalType, mixType);
        }

        public static IMixEvaluator RangeMixEntryModelToMixEvaluator(MixType mixType, string input)
        {
            PropertyInfo info = null;
            switch (mixType & MixType.SUBTYPE_MASK)
            {
                case MixType.LASTPLAYED_SUBTYPE:
                    info = typeof(SongViewModel).GetRuntimeProperty(SongViewModel.Properties.LastPlayed);
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected RANGE_TYPE MixType {0}", mixType);
                    break;
            }

            RangeEvalType rangeEvalType = RangeEvalType.Unknown;
            switch (mixType & MixType.VARIANT_MASK)
            {
                case MixType.RANGE_DAYS_VARIANT:
                    rangeEvalType = RangeEvalType.Days;
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected RANGE_TYPE VARIANT_MASK MixType {0}", mixType);
                    break;
            }

            if (info != null)
            {
                return new RangeMixEvaluator(info, int.Parse(input), rangeEvalType, mixType);
            }

            return null;
        }

        #endregion

        #region Methods

        private bool MixSortOrderToIsAscending(MixSortOrder mixSortOrder)
        {
            return ((mixSortOrder & MixSortOrder.ORDER_MASK) == MixSortOrder.SORTORDER_ASC);
        }

        private string MixSortOrderToSongProperty(MixSortOrder mixSortOrder)
        {
            switch (mixSortOrder & MixSortOrder.PROPERTY_MASK)
            {
                case MixSortOrder.ALBUMARTISTNAMESORT:
                    return SongViewModel.Properties.AlbumArtistSortName;
                case MixSortOrder.ALBUMNAMESORT:
                    return SongViewModel.Properties.AlbumSortName;
                case MixSortOrder.ARTISTNAMESORT:
                    return SongViewModel.Properties.ArtistSortName;
                case MixSortOrder.DURATIONSORT:
                    return SongViewModel.Properties.Duration;
                case MixSortOrder.LASTPLAYEDSORT:
                    return SongViewModel.Properties.LastPlayed;
                case MixSortOrder.PLAYCOUNTSORT:
                    return SongViewModel.Properties.PlayCount;
                case MixSortOrder.RATINGSORT:
                    return SongViewModel.Properties.Rating;
                case MixSortOrder.TRACKNAMESORT:
                    return SongViewModel.Properties.SortName;
                default:
                    DebugHelper.Assert(new CallerInfo(), mixSortOrder == MixSortOrder.None, "Unexpected mix sort order");
                    return SongViewModel.Properties.SongId;
            }
        }

        public void Initalize()
        {
            UpdateDependencyList();

            CurrentSongs.CollectionChanged += HandleCurrentSongsCollectionChanged;

            foreach (SongViewModel song in LibraryViewModel.Current.FlatSongCollection)
            {
                ReevaulateSong(song);
            }

            LibraryViewModel.Current.FlatSongCollection.CollectionChanged += HandleFlatSongCollectionChanged;

            ResetLength();
        }

        public void Reset()
        {
            CurrentSongs.Clear();

            CurrentSongs.CollectionChanged -= HandleCurrentSongsCollectionChanged;

            CurrentSongs.UpdateLimit((uint)Limit, HasLimit);
            CurrentSongs.UpdateSortFunction(new SongSortGenericOrder(MixSortOrderToSongProperty(SortType), MixSortOrderToIsAscending(SortType)));

            List<MixViewModel> oldDependentMixes = new List<MixViewModel>(DependentMixes);

            UpdateDependencyList();

            if (!IsCircular)
            {
                foreach (SongViewModel song in LibraryViewModel.Current.FlatSongCollection)
                {
                    if (RootEvaluator.Eval(song))
                    {
                        CurrentSongs.Add(song);
                    }
                }
            }

            // Reset any mixes that used to depend on this and might not anymore
            foreach (MixViewModel mixViewModel in oldDependentMixes)
            {
                if (!DependentMixes.Contains(mixViewModel))
                {
                    mixViewModel.Reset();
                }
            }

            if (!IsCircular)
            {
                // Reset any mixes that now depend on this
                List<MixViewModel> copyDependentMixes = new List<MixViewModel>(DependentMixes);

                foreach (MixViewModel mixViewModel in copyDependentMixes)
                {
                    mixViewModel.Reset();
                }
            }

            CurrentSongs.CollectionChanged += HandleCurrentSongsCollectionChanged;

            ResetLength();
        }

        public void ReevaulateSong(SongViewModel song)
        {
            bool belongsInList = !IsCircular && RootEvaluator.Eval(song);

            if (belongsInList && !CurrentSongs.ActuallyContains(song))
            {
                CurrentSongs.Add(song);
            }
            else if (!belongsInList && CurrentSongs.ActuallyContains(song))
            {
                CurrentSongs.Remove(song);
            }
        }

        public void UpdateDependencyList()
        {
            foreach (PlaylistViewModel playlistViewModel in LibraryViewModel.Current.PlaylistCollection)
            {
                playlistViewModel.DependentMixes.Remove(this);

                if (RootEvaluator.IsPlaylistNested(playlistViewModel.PlaylistId))
                {
                    playlistViewModel.DependentMixes.Add(this);
                }
            }

            IsCircular = false;

            foreach (MixViewModel mixViewModel in LibraryViewModel.Current.MixCollection)
            {
                mixViewModel.DependentMixes.Remove(this);

                if (RootEvaluator.IsMixNested(mixViewModel.MixId))
                {
                    if (mixViewModel == this)
                    {
                        MarkAsCircular();
                    }

                    mixViewModel.DependentMixes.Add(this);
                }
            }

            UpdateCircular(new List<MixViewModel>());
        }

        public bool UpdateCircular(List<MixViewModel> treeSoFar)
        {
            IsCircular = false;

            treeSoFar.Add(this);

            foreach (MixViewModel candidate in treeSoFar)
            {
                if (DependentMixes.Contains(candidate))
                {
                    MarkAsCircular();

                    return true;
                }
            }

            foreach (MixViewModel dependentMix in DependentMixes)
            {
                if (dependentMix.UpdateCircular(new List<MixViewModel>(treeSoFar)))
                {
                    MarkAsCircular();

                    return true;
                }
            }

            return false;

        }

        public void MarkAsCircular()
        {
            // already marked
            if (IsCircular) return;

            IsCircular = true;

            foreach (MixViewModel dependentMix in DependentMixes)
            {
                dependentMix.MarkAsCircular();
            }
        }

        public void OnSongPropertyChanged(SongViewModel song, string propertyName)
        {
            if (RootEvaluator.IsPropertyAffected(propertyName))
            {
                ReevaulateSong(song);
            }

            if (propertyName == MixSortOrderToSongProperty(SortType))
            {
                // Remove and reinsert to update sort order
                if (CurrentSongs.ActuallyContains(song))
                {
                    CurrentSongs.Remove(song);
                    CurrentSongs.Add(song);
                }
            }
        }

        internal bool ContainsSong(SongViewModel song)
        {
            return CurrentSongs.Contains(song);
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
            int limit = int.Parse(DebugHelper.CastAndAssert<string>(parameter));

            LibraryViewModel.Current.PlayQueue.PlaySongList(CurrentSongs, true, limit);
        }

        private bool CanExecutePlayAllSongs(object parameter)
        {
            int limit = int.Parse(DebugHelper.CastAndAssert<string>(parameter));

            return CurrentSongs.Count > limit;
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
            int limit = int.Parse(DebugHelper.CastAndAssert<string>(parameter));

            LibraryViewModel.Current.PlayQueue.PlaySongList(CurrentSongs, false, limit);
        }

        private bool CanExecuteQueueAllSongs(object parameter)
        {
            int limit = int.Parse(DebugHelper.CastAndAssert<string>(parameter));

            return CurrentSongs.Count > limit;
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

            LibraryViewModel.Current.PlayQueue.ShuffleSongList(CurrentSongs, true, limit);
        }

        private bool CanExecuteShuffleAllSongs(object parameter)
        {
            int limit = int.Parse(DebugHelper.CastAndAssert<string>(parameter));

            return CurrentSongs.Count > limit;
        }


        private RelayCommand _editMix;
        public RelayCommand EditMix
        {
            get
            {
                if (_editMix == null) _editMix = new RelayCommand(CanExecuteEditMix, ExecuteEditMix);

                return _editMix;
            }
        }

        private async void ExecuteEditMix(object parameter)
        {
            EditMix editPlaylistDialog = new EditMix(this);

            await editPlaylistDialog.ShowAsync();
        }

        private bool CanExecuteEditMix(object parameter)
        {
            return true;
        }

        #endregion

        /*
        internal void DeleteSelf()
        {
            LibraryViewModel.Current.RemoveMix(rootModel);

            rootModel.DeleteSelf();
        }

        internal void Save()
        {
            if (RootType != null)
            {
                string s = RootType.Save(this.Id);

                Logger.Current.Log(string.Format("Saving Mix {0} root {1} result {2}", RootType.Id, this.Id, s));
            }
            else
            {
                Logger.Current.Log("Saving Mix NULL root " + this.Id);
            }
        }





        internal bool IsPlaylistNested(int targetId)
        {
            return RootType.IsPlaylistNested(targetId);
        }

        internal bool IsMixNested(int targetId)
        {
            return RootType.IsMixNested(targetId);
        }

        internal bool ContainsSong(SongViewModel song)
        {
            return CurrentSongs.Contains(song);
        }
*/

        internal void SetEvaluator(IMixEvaluator mixEvaluator)
        {
            // TODO: probably a better way of handling this
            rootModel.ClearMixEntries();

            mixEvaluator.Save(rootModel.MixId, true);

            rootModel.UpdateMixEntries();

            RootEvaluator = mixEvaluator;
        }
    }
}

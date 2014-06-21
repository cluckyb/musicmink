using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Helpers;
using MusicMinkAppLayer.PlayQueue;
using MusicMinkAppLayer.Tables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace MusicMinkAppLayer.Models
{
    /// <summary>
    /// Handles communicating with the background process to provide information about currently playing track
    /// </summary>
    public class PlayQueueModel : RootModel
    {
        private const int MAX_QUEUE_SIZE = 100;
        private AutoResetEvent BackgroundInitialized;
        private DispatcherTimer progressUpdateTimer;

        private Dictionary<int, PlayQueueEntryModel> LookupMap = new Dictionary<int, PlayQueueEntryModel>();

        public static class Properties
        {
            public const string NowPlaying = "NowPlaying";
            public const string PrevTrack = "PrevTrack";
            public const string NextTrack = "NextTrack";
            public const string CurrentPlaybackQueueEntryId = "CurrentPlaybackQueueEntryId";
            public const string CurrentTime = "CurrentTime";
            public const string FullTime = "FullTime";
            public const string IsPlaying = "IsPlaying";
            public const string IsActive = "IsActive";
        }

        internal PlayQueueModel()
        {
            progressUpdateTimer = new DispatcherTimer();
            progressUpdateTimer.Tick += HandleProgressTimerTick;
            progressUpdateTimer.Interval = TimeSpan.FromSeconds(0.25);

            BackgroundInitialized = new AutoResetEvent(false);
        }

        public void Start()
        {
            Populate();
           
            Init();
        }

        #region Event Handlers

        private void HandleProgressTimerTick(object sender, object e)
        {
            if (!isScrubInProgress && IsActive)
            {
                CurrentTime = BackgroundMediaPlayer.Current.Position;
            }
        }

        async void HandleBackgroundMediaPlayerMessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key)
                {
                    case PlayQueueMessageHelper.BackgroundStarted:
                        BackgroundInitialized.Set();
                        break;
                    case PlayQueueMessageHelper.SeekComplete:
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            isScrubInProgress = false;
                            CurrentTime = BackgroundMediaPlayer.Current.Position;
                        });
                        break;
                    case PlayQueueMessageHelper.TrackChanged:
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (!isScrubInProgress)
                            {
                                CurrentTime = BackgroundMediaPlayer.Current.Position;
                            }
                            else
                            {
                                CurrentTime = TimeSpan.Zero;
                            }
                            FullTime = BackgroundMediaPlayer.Current.NaturalDuration;

                            CurrentPlaybackQueueEntryId = DebugHelper.CastAndAssert<int>(e.Data[key]);
                        });
                        break;
                    case PlayQueueMessageHelper.TrackPlayed:
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            UpdateHistory();
                        });
                        break;
                    case PlayQueueMessageHelper.PlayQueueFinished:
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            ResetPlayerToStart();
                            IsActive = false;
                            IsPlaying = false;
                            CurrentTime = TimeSpan.Zero;
                        });
                        break;
                }
            }
        }

        private void HandleBackgroundTaskInitializationCompleted(IAsyncAction asyncInfo, AsyncStatus asyncStatus)
        {
            if (asyncStatus == AsyncStatus.Completed)
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Background Audio Task was initalized");
            }
            else
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Error, "Background Audio Task was NOT initalized due to error {0}", asyncInfo.ErrorCode.ToString());
            }
        }

        #endregion

        #region Private Properties / Members

        private bool IsBackgroundStarted = false;

        private bool IsBackgroundAudioTaskRunning
        {
            get
            {
                return ApplicationSettings.GetSettingsValue<bool>(ApplicationSettings.IS_BACKGROUND_PROCESS_ACTIVE, false);
            }
        }

        private PlayQueueEntryModel _currentEntry;
        private PlayQueueEntryModel CurrentEntry
        {
            get
            {
                return _currentEntry;
            }
            set
            {
                if (_currentEntry != value)
                {
                    if (_currentEntry != null)
                    {
                        _currentEntry.PropertyChanged -= HandleCurrentEntryPropertyChanged;
                    }

                    _currentEntry = value;
                    UpdateFullTime();

                    if (_currentEntry != null)
                    {
                        _currentEntry.PropertyChanged += HandleCurrentEntryPropertyChanged;
                    }
                }
            }
        }

        void HandleCurrentEntryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case PlayQueueEntryModel.Properties.NextId:
                case PlayQueueEntryModel.Properties.PrevId:
                    SendMessageToBackground(PlayQueueConstantFGMessageId.PossibleChromeChange);
                    break;
            }
        }

        #endregion

        #region Properties

        private ObservableCollection<PlayQueueEntryModel> _playbackQueue = new ObservableCollection<PlayQueueEntryModel>();
        public ObservableCollection<PlayQueueEntryModel> PlaybackQueue
        {
            get
            {
                return _playbackQueue;
            }
        }

        private bool isScrubInProgress = false;
        private TimeSpan _currentTime = TimeSpan.Zero;
        public TimeSpan CurrentTime
        {
            get
            {
                return _currentTime;
            }
            protected set
            {
                if (_currentTime != value)
                {
                    _currentTime = value;
                    NotifyPropertyChanged(Properties.CurrentTime);
                }
            }
        }

        private TimeSpan _fullTime = TimeSpan.Zero;
        public TimeSpan FullTime
        {
            get
            {
                return _fullTime;
            }
            protected set
            {
                if (_fullTime != value)
                {
                    _fullTime = value;
                    NotifyPropertyChanged(Properties.FullTime);
                }
            }
        }

        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            protected set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    NotifyPropertyChanged(Properties.IsPlaying);

                    if (IsPlaying)
                    {
                        progressUpdateTimer.Start();
                    }
                    else
                    {
                        if (!isScrubInProgress && IsActive)
                        {
                            CurrentTime = BackgroundMediaPlayer.Current.Position;
                        }
                        progressUpdateTimer.Stop();
                    }
                }
            }
        }

        private bool _isActive = false;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            protected set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    NotifyPropertyChanged(Properties.IsActive);
                }
            }
        }

        public int PrevTrack
        {
            get
            {
                if (CurrentPlaybackQueueEntryId == 0) return 0;

                int prevId = LookupMap[CurrentPlaybackQueueEntryId].PrevId;

                if (prevId == 0) return 0;

                return LookupMap[prevId].SongId;
            }
        }

        public int NowPlaying
        {
            get
            {
                if (LookupMap.Count == 0) return 0;

                if (CurrentPlaybackQueueEntryId == 0) return 0;

                return LookupMap[CurrentPlaybackQueueEntryId].SongId;
            }
        }

        public int NextTrack
        {
            get
            {
                if (CurrentPlaybackQueueEntryId == 0) return 0;

                int nextId = LookupMap[CurrentPlaybackQueueEntryId].NextId;

                if (nextId == 0) return 0;

                return LookupMap[nextId].SongId;
            }
        }

        private int _currentPlaybackQueueEntryId = 0;
        public int CurrentPlaybackQueueEntryId
        {
            get
            {
                return _currentPlaybackQueueEntryId;
            }
            set
            {
                if (ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0) != value)
                {
                    ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, value);
                }

                if (_currentPlaybackQueueEntryId != value)
                {
                    _currentPlaybackQueueEntryId = value;

                    if (_currentPlaybackQueueEntryId > 0)
                    {
                        DebugHelper.Assert(new CallerInfo(), LookupMap.ContainsKey(_currentPlaybackQueueEntryId));

                        if (LookupMap.ContainsKey(_currentPlaybackQueueEntryId))
                        {
                            CurrentEntry = LookupMap[_currentPlaybackQueueEntryId];
                        }
                    }

                    NotifyPropertyChanged(Properties.CurrentPlaybackQueueEntryId);
                    NotifyPropertyChanged(Properties.PrevTrack);
                    NotifyPropertyChanged(Properties.NowPlaying);
                    NotifyPropertyChanged(Properties.NextTrack);
                }
            }
        }

        #endregion 

        #region Methods

        object historyLock = new object();
        bool isUpdatingHistory = false;
        private async void UpdateHistory()
        {
            bool canRun = false;

            lock (historyLock)
            {
                if (!isUpdatingHistory)
                {
                    isUpdatingHistory = true;
                    canRun = true;
                }
            }

            if (!canRun) return;

            List<HistoryTable> historyItems = DatabaseManager.Current.FetchHistoryItems();

            // Process everything first so we don't have to wait for scrobbles to see those updates
            foreach (HistoryTable historyItem in historyItems)
            {
                SongModel song = LibraryModel.Current.LookupSongById(historyItem.SongId);

                if (song != null)
                {
                    if (!historyItem.Processed)
                    {
                        song.PlayCount++;

                        if (song.LastPlayed < historyItem.DatePlayed)
                        {
                            song.LastPlayed = historyItem.DatePlayed;
                        }

                        historyItem.Processed = true;
                    }
                }
            }

            // TODO: #3 can batch scrobbles
            foreach (HistoryTable historyItem in historyItems)
            {
                if (!historyItem.Scrobbled)
                {
                    LastfmStatusCode scrobbleResult = await LastFMManager.Current.ScrobbleTrack(historyItem.SongName, historyItem.ArtistName, new DateTime(historyItem.DatePlayed));

                    Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Scrobbling row {0} result {1}", historyItem.RowId, scrobbleResult);

                    // "Failure" is non-recoverable, so actually mark it as scrobbled so we don't retry later
                    if (scrobbleResult == LastfmStatusCode.Success ||
                        scrobbleResult == LastfmStatusCode.Failure)
                    {
                        historyItem.Scrobbled = true;
                    }
                }

                if (historyItem.Processed && historyItem.Scrobbled)
                {
                    DatabaseManager.Current.DeleteHistoryItem(historyItem.RowId);
                }
                else
                {
                    DatabaseManager.Current.Update(historyItem);
                }
            }

            lock (historyLock)
            {
                isUpdatingHistory = false;
            }
        }

        public void Populate()
        {
            List<PlayQueueEntryTable> allEntries = DatabaseManager.Current.FetchPlayQueueEntries();

            PlayQueueEntryModel head = null;

            foreach (PlayQueueEntryTable playQueueEntry in allEntries)
            {
                PlayQueueEntryModel newEntry = new PlayQueueEntryModel(playQueueEntry);

                LookupMap.Add(newEntry.RowId, newEntry);

                if (newEntry.PrevId == 0)
                {
                    DebugHelper.Assert(new CallerInfo(), head == null, "Second head found in play queue!!!");

                    head = newEntry;
                }
            }

            PlayQueueEntryModel currentLocation = head;

            while (currentLocation != null && PlaybackQueue.Count < LookupMap.Count)
            {
                PlaybackQueue.Add(currentLocation);

                if (LookupMap.ContainsKey(currentLocation.NextId))
                {
                    currentLocation = LookupMap[currentLocation.NextId];
                }
                else
                {
                    currentLocation = null;
                }
            }

            DebugHelper.Assert(new CallerInfo(), currentLocation == null, "Circular reference found in Play Queue");
            DebugHelper.Assert(new CallerInfo(), PlaybackQueue.Count == LookupMap.Count, "Missing element found in Play Queue");

            CurrentPlaybackQueueEntryId = ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);

            if (CurrentPlaybackQueueEntryId == 0)
            {
                ResetPlayerToStart();
            }
        }

        public void ResetPlayerToStart()
        {
            if (PlaybackQueue.Count > 0)
            {
                PlayQueueEntryModel playQueueEntry = PlaybackQueue.First();

                CurrentPlaybackQueueEntryId = playQueueEntry.RowId;
            }
        }

        private void Stop()
        {
            BackgroundMediaPlayer.Current.Pause();

            return;
        }

        public void ClearQueue()
        {
            Stop();

            CurrentPlaybackQueueEntryId = 0;
            PlaybackQueue.Clear();
            LookupMap.Clear();

            DatabaseManager.Current.ClearPlaybackQueue();
        }

        public void RemoveSong(int songId)
        {
            List<PlayQueueEntryTable> entriesToRemove = DatabaseManager.Current.LookupPlayQueueEntryBySongId(songId);

            foreach (PlayQueueEntryTable entry in entriesToRemove)
            {
                RemoveEntry(entry.RowId);
            }
        }

        public void RemoveEntry(int entryId)
        {
            if (entryId == CurrentPlaybackQueueEntryId)
            {
                Stop();
                CurrentPlaybackQueueEntryId = 0;
            }

            PlayQueueEntryModel songToRemove = null;

            if (!LookupMap.TryGetValue(entryId, out songToRemove))
            {
                DebugHelper.Alert(new CallerInfo(), "Tried to remove play queue entry {0} but its not in our lookup", entryId);

                return;
            }

            PlayQueueEntryModel previousModel = null;

            if (LookupMap.TryGetValue(songToRemove.PrevId, out previousModel))
            {
                previousModel.NextId = songToRemove.NextId;
            }

            PlayQueueEntryModel nextModel = null;

            if (LookupMap.TryGetValue(songToRemove.NextId, out nextModel))
            {
                nextModel.PrevId = songToRemove.PrevId;
            }

            PlaybackQueue.Remove(songToRemove);
            LookupMap.Remove(songToRemove.RowId);
            DatabaseManager.Current.DeletePlayQueueEntry(songToRemove.RowId);

            NotifyPropertyChanged(Properties.NextTrack);
            NotifyPropertyChanged(Properties.PrevTrack);
        }

        public void MoveSong(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;

            PlayQueueEntryModel songToMove = PlaybackQueue[oldIndex];
            PlayQueueEntryModel target = null;

            if (newIndex > 0)
            {
                if (newIndex < oldIndex)
                {
                    target = PlaybackQueue[newIndex - 1];
                }
                else
                {
                    target = PlaybackQueue[newIndex];
                }
            }

            // Remove from old spot
            PlayQueueEntryModel previousModel = null;

            if (LookupMap.TryGetValue(songToMove.PrevId, out previousModel))
            {
                previousModel.NextId = songToMove.NextId;
            }

            PlayQueueEntryModel nextModel = null;

            if (LookupMap.TryGetValue(songToMove.NextId, out nextModel))
            {
                nextModel.PrevId = songToMove.PrevId;
            }

            // Insert after new spot
            if (target == null)
            {
                PlayQueueEntryModel head = null;

                if (PlaybackQueue.Count > 0)
                {
                    head = PlaybackQueue.First();
                }

                if (head != null)
                {
                    songToMove.NextId = head.RowId;
                    head.PrevId = songToMove.RowId;
                    songToMove.PrevId = 0;
                }
                else
                {
                    // Should be redundant
                    songToMove.NextId = 0;
                    songToMove.PrevId = 0;
                }
            }
            else
            {
                PlayQueueEntryModel newNextModel = null;

                if (LookupMap.TryGetValue(target.NextId, out newNextModel))
                {
                    newNextModel.PrevId = songToMove.RowId;
                }

                songToMove.NextId = target.NextId;
                target.NextId = songToMove.RowId;
                songToMove.PrevId = target.RowId;
            }

            PlaybackQueue.Move(oldIndex, newIndex);

            NotifyPropertyChanged(Properties.NextTrack);
            NotifyPropertyChanged(Properties.PrevTrack);
        }

        private int AddSong(int songId)
        {
            // Keep queue trimmed
            if (LookupMap.Count >= MAX_QUEUE_SIZE)
            {
                PlayQueueEntryModel head = null;

                if (PlaybackQueue.Count > 0)
                {
                    head = PlaybackQueue.First();
                }

                if (head != null)
                {
                    if (CurrentPlaybackQueueEntryId == head.RowId)
                    {
                        // TODO: #6 raise alert about queue being full
                        return -1;
                    }
                    else
                    {
                        RemoveEntry(head.RowId);
                    }
                }
            }

            PlayQueueEntryModel currentTail = null;

            if (PlaybackQueue.Count > 0)
            {
                currentTail = PlaybackQueue.Last();
            }

            PlayQueueEntryTable newPlayQueueEntry;

            if (currentTail == null)
            {
                newPlayQueueEntry = new PlayQueueEntryTable(songId, 0, 0);

                DatabaseManager.Current.AddPlayQueueEntry(newPlayQueueEntry);
            }
            else
            {
                newPlayQueueEntry = new PlayQueueEntryTable(songId, 0, currentTail.RowId);
               
                DatabaseManager.Current.AddPlayQueueEntry(newPlayQueueEntry);

                currentTail.NextId = newPlayQueueEntry.RowId;
            }

            PlayQueueEntryModel newEntry = new PlayQueueEntryModel(newPlayQueueEntry);

            LookupMap.Add(newEntry.RowId, newEntry);
            PlaybackQueue.Add(newEntry);

            NotifyPropertyChanged(Properties.NextTrack);
            NotifyPropertyChanged(Properties.PrevTrack);

            return newEntry.RowId;
        }

        public void PlaySong(int songId)
        {
            ClearQueue();

            AddSong(songId);

            Play();
        }

        public void QueueSong(int songId)
        {
            AddSong(songId);
        }

        private void Play()
        {
            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Current state when play: {0} IsBackgroundRunning: {1}", BackgroundMediaPlayer.Current.CurrentState, IsBackgroundAudioTaskRunning);

            SentMessageToBackgroundOrInit(PlayQueueConstantFGMessageId.StartPlayback);
        }

        public void Resume()
        {
            CurrentPlaybackQueueEntryId = ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);

            if (CurrentPlaybackQueueEntryId == 0)
            {
                ResetPlayerToStart();
            }

            Init();
        }

        private void Init()
        {
            if (IsBackgroundAudioTaskRunning)
            {
                IsBackgroundStarted = true;

                UpdatePlayingAndActivity(BackgroundMediaPlayer.Current);

                AttachBackgroundMediaPlayerEventHandlers();

                SendMessageToBackground(PlayQueueConstantFGMessageId.AppResumed);
            }
            else
            {
                IsActive = false;
               
                IsBackgroundStarted = false;
            }

            UpdateFullTime();
            double percentage = ApplicationSettings.GetSettingsValue<double>(ApplicationSettings.CURRENT_TRACK_PERCENTAGE, 0.0);

            CurrentTime = TimeSpan.FromTicks((long)(FullTime.Ticks * percentage));

            UpdateHistory();

            ApplicationSettings.PutSettingsValue(ApplicationSettings.IS_FOREGROUND_PROCESS_ACTIVE, true);
        }

        public void Suspend()
        {
            SendMessageToBackground(PlayQueueConstantFGMessageId.AppSuspended);

            DetachBackgroundMediaPlayerEventHandlers();

            ApplicationSettings.PutSettingsValue(ApplicationSettings.IS_FOREGROUND_PROCESS_ACTIVE, false);
        }

        void AttachBackgroundMediaPlayerEventHandlers()
        {
            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Events attached");

            BackgroundMediaPlayer.MessageReceivedFromBackground -= HandleBackgroundMediaPlayerMessageReceivedFromBackground;
            BackgroundMediaPlayer.MessageReceivedFromBackground += HandleBackgroundMediaPlayerMessageReceivedFromBackground;

            BackgroundMediaPlayer.Current.CurrentStateChanged -= HandleBackgroundMediaPlayerCurrentStateChanged;
            BackgroundMediaPlayer.Current.CurrentStateChanged += HandleBackgroundMediaPlayerCurrentStateChanged;
        }

        async void HandleBackgroundMediaPlayerCurrentStateChanged(MediaPlayer sender, object args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdatePlayingAndActivity(sender);
            });
        }

        private void UpdatePlayingAndActivity(MediaPlayer sender)
        {
            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Changed state: {0} Is End: {1}", sender.CurrentState, sender.NaturalDuration.Ticks == sender.Position.Ticks);

            if (sender.CurrentState == MediaPlayerState.Playing || (IsActive && sender.CurrentState == MediaPlayerState.Paused && sender.NaturalDuration.Ticks == sender.Position.Ticks))
            {
                IsPlaying = true;
            }
            else
            {
                IsPlaying = false;
            }

            if (sender.CurrentState == MediaPlayerState.Paused ||
                sender.CurrentState == MediaPlayerState.Playing)
            {
                IsActive = true;
                FullTime = BackgroundMediaPlayer.Current.NaturalDuration;
            }
            else
            {
                IsActive = false;

                UpdateFullTime();
            }
        }

        private void UpdateFullTime()
        {
            if (CurrentEntry == null)
            {
                FullTime = TimeSpan.Zero;
            }
            else
            {
                SongTable song = DatabaseManager.Current.LookupSongById(CurrentEntry.SongId);

                if (song == null)
                {
                    DebugHelper.Alert(new CallerInfo(), "SongId {0} got from a playqueue row but couldn't find it in database!!", CurrentEntry.SongId);
                    FullTime = TimeSpan.Zero;
                }
                else
                {
                    FullTime = TimeSpan.FromTicks(song.Duration);
                }
            }
        }

        void DetachBackgroundMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground -= HandleBackgroundMediaPlayerMessageReceivedFromBackground;
            BackgroundMediaPlayer.Current.CurrentStateChanged -= HandleBackgroundMediaPlayerCurrentStateChanged;
        }

        private void StartBackgroundTask(PlayQueueConstantFGMessageId message)
        {
            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Starting background task...");
            AttachBackgroundMediaPlayerEventHandlers();

            var backgroundtaskinitializationresult = CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool result = BackgroundInitialized.WaitOne(20000);
                if (result == true)
                {
                    IsBackgroundStarted = true;

                    SendMessageToBackground(message);
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            }
            );
            backgroundtaskinitializationresult.Completed += HandleBackgroundTaskInitializationCompleted;
        }

        public void InformBackgroundLoggingChanged()
        {
            SentMessageToBackgroundOrInit(PlayQueueConstantFGMessageId.LoggingEnabledChanged);
        }

        public void PlayPause()
        {
            SentMessageToBackgroundOrInit(PlayQueueConstantFGMessageId.PlayPauseTrack);
        }

        public void Skip()
        {
            SentMessageToBackgroundOrInit(PlayQueueConstantFGMessageId.SkipTrack);
        }

        public void GoBack()
        {
            SentMessageToBackgroundOrInit(PlayQueueConstantFGMessageId.PrevTrack);
        }

        public void PlayFromSong(int rowId)
        {
            Stop();

            CurrentPlaybackQueueEntryId = rowId;

            SentMessageToBackgroundOrInit(PlayQueueConstantFGMessageId.StartPlayback);
        }

        public void ScrubToPercentage(double percentage)
        {
            CurrentTime = TimeSpan.FromTicks((long)(FullTime.Ticks * percentage));

            if (IsActive)
            {
                isScrubInProgress = true;

                SendMessageToBackground(PlayQueueConstantFGMessageId.ScrubToPercentage, percentage);
            }
            else
            {
                ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_TRACK_PERCENTAGE, percentage);
            }
        }

        #endregion

        #region Message Passing

        private void SendMessageToBackground(PlayQueueConstantFGMessageId messageId, object value)
        {
            var message = new ValueSet();
            message.Add(PlayQueueMessageHelper.FGMessageIdToMessageString(messageId), value);
            BackgroundMediaPlayer.SendMessageToBackground(message);
        }

        private void SendMessageToBackground(PlayQueueConstantFGMessageId messageId)
        {
            SendMessageToBackground(messageId, DateTime.Now.ToString());
        }

        private void SentMessageToBackgroundOrInit(PlayQueueConstantFGMessageId messageId)
        {
            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Sending Message To Background Message: {0} IsRunning: {1}", messageId.ToString(), IsBackgroundAudioTaskRunning);

            if (IsBackgroundStarted)
            {
                SendMessageToBackground(messageId);
            }
            else
            {
                StartBackgroundTask(messageId);
            }
        }

        #endregion
    }
}
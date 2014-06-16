using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Helpers;
using MusicMinkAppLayer.Tables;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;

namespace MusicMinkAppLayer.PlayQueue
{
    /// <summary>
    /// Used by the Background Process to work the play queue (Foreground process uses PlayQueueModel)
    /// </summary>
    public class PlayQueueManager
    {
        private static PlayQueueManager _current;
        public static PlayQueueManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new PlayQueueManager();
                }

                return _current;
            }
        }

        private MediaPlayer mediaPlayer;

        public event TypedEventHandler<PlayQueueManager, TrackInfo> TrackChanged;

        public bool IsActive { get; private set; }

        private PlayQueueManager()
        {
            mediaPlayer = BackgroundMediaPlayer.Current;
            mediaPlayer.MediaPlayerRateChanged += HandleMediaPlayerMediaPlayerRateChanged;
            mediaPlayer.MediaFailed += HandleMediaPlayerMediaFailed;
            mediaPlayer.MediaEnded += HandleMediaPlayerMediaEnded;
            mediaPlayer.MediaOpened += HandleMediaPlayerMediaOpened;

            mediaPlayer.AutoPlay = true;

            IsActive = false;
        }

        #region Event Handlers

        private void HandleMediaPlayerMediaPlayerRateChanged(MediaPlayer sender, MediaPlayerRateChangedEventArgs args)
        {
        }

        private void HandleMediaPlayerMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Media Player Failed With Error code {0}", args.ExtendedErrorCode.ToString());

        }

        private TrackInfo playingTrack = null;

        private void HandleMediaPlayerMediaEnded(MediaPlayer sender, object args)
        {
            DebugHelper.Assert(new CallerInfo(), playingTrack != null);

            if (playingTrack != null)
            {
                DatabaseManager.Current.AddHistoryItem(new HistoryTable(playingTrack.SongId, false, false, DateTime.UtcNow.Ticks, playingTrack.Title, playingTrack.Artist));

                SendMessageToForeground(PlayQueueConstantBGMessageId.TrackPlayed);
            }
           
            PlayNext();
        }

        private bool isFirstOpen = false;
        void HandleMediaPlayerMediaOpened(MediaPlayer sender, object args)
        {
            if (isFirstOpen)
            {
                isFirstOpen = false;
                double percentage = ApplicationSettings.GetSettingsValue<double>(ApplicationSettings.CURRENT_TRACK_PERCENTAGE, 0);
                ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_TRACK_PERCENTAGE, 0.0);

                if (percentage > 0)
                {
                    Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Length Total {0}", mediaPlayer.NaturalDuration.Ticks);

                    mediaPlayer.Position = TimeSpan.FromTicks((long)(mediaPlayer.NaturalDuration.Ticks * percentage));
                }
            }


            int trackId = ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);

            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Trying to play row {0}", trackId);

            playingTrack = TrackInfo.TrackInfoFromRowId(trackId);
            TrackChanged.Invoke(this, playingTrack);
        }

        #endregion

        #region Methods

        public bool CanSkip()
        {
            int rowId = ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);

            PlayQueueEntryTable currentPlayQueueRow = DatabaseManager.Current.LookupPlayQueueEntryById(rowId);

            return (currentPlayQueueRow != null && currentPlayQueueRow.NextId > 0);
        }

        public bool CanBack()
        {
            int rowId = ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);

            PlayQueueEntryTable currentPlayQueueRow = DatabaseManager.Current.LookupPlayQueueEntryById(rowId);

            return (currentPlayQueueRow != null && currentPlayQueueRow.PrevId > 0);
        }

        public async void PlayNext()
        {
            int rowId = ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);

            PlayQueueEntryTable currentPlayQueueRow = DatabaseManager.Current.LookupPlayQueueEntryById(rowId);

            if (currentPlayQueueRow != null)
            {
                int nextRowId = currentPlayQueueRow.NextId;
                ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, nextRowId);
            }
            else
            {
                PlayQueueEntryTable head = DatabaseManager.Current.LookupPlayQueueEntryHead();

                if (head != null)
                {
                    ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, head.RowId);
                }
                else
                {
                    ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);
                }
            }

            await PlayCurrent();
        }

        public async void PlayPrev()
        {
            int rowId = ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);

            PlayQueueEntryTable currentPlayQueueRow = DatabaseManager.Current.LookupPlayQueueEntryById(rowId);

            int prevRowId = 0;
            if (currentPlayQueueRow != null)
            {
                prevRowId = currentPlayQueueRow.PrevId;
            }

            ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, prevRowId);

            await PlayCurrent();
        }

        async Task PlayCurrent()
        {
            int rowId = ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);

            TrackInfo trackInfo = TrackInfo.TrackInfoFromRowId(rowId);

            // TODO: #18  Support other types
            if (trackInfo != null)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(trackInfo.Path);

                isFirstOpen = true;
                mediaPlayer.SetFileSource(file);
                IsActive = true;

                mediaPlayer.Play();
            }
            else
            {
                IsActive = false;
            }
        }

        public async void Play()
        {
            int rowId = ApplicationSettings.GetSettingsValue<int>(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);

            if (rowId == 0)
            {
                ResetAndPlay();
            }
            else
            {
                await PlayCurrent();
            }
        }

        public async void ResetAndPlay()
        {
            PlayQueueEntryTable head = DatabaseManager.Current.LookupPlayQueueEntryHead();

            if (head != null)
            {
                ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, head.RowId);
            }
            else
            {
                ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_PLAYQUEUE_POSITION, 0);
            }

            await PlayCurrent();
        }

        public void Connect()
        {
            DatabaseManager.Current.Connect();
        }

        public void Disconnect()
        {
            if (mediaPlayer.NaturalDuration.TotalSeconds > 0)
            {
                double percentage = mediaPlayer.Position.TotalSeconds / mediaPlayer.NaturalDuration.TotalSeconds;
                ApplicationSettings.PutSettingsValue(ApplicationSettings.CURRENT_TRACK_PERCENTAGE, percentage);
            }

            IsActive = false;

            DatabaseManager.Current.Disconnect();
        }

        #endregion

        public void SendMessageToForeground(PlayQueueConstantBGMessageId messageId, object value)
        {
            var message = new ValueSet();
            message.Add(PlayQueueMessageHelper.BGMessageIdToMessageString(messageId), value);
            BackgroundMediaPlayer.SendMessageToForeground(message);
        }

        public void SendMessageToForeground(PlayQueueConstantBGMessageId messageId)
        {
            SendMessageToForeground(messageId, DateTime.Now.ToString());
        }
    }
}

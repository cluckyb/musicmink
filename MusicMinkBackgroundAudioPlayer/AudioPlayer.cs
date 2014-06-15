using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Helpers;
using MusicMinkAppLayer.PlayQueue;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.StartScreen;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace MusicMinkBackgroundAudioPlayer
{
    /// <summary>
    /// Background task that handles managing all of the audio playback with the help of PlayQueueManager
    /// </summary>
    public sealed class AudioPlayer : IBackgroundTask
    {
        private enum BackgroundTaskState
        {
            Stopped,
            Running
        }

        private enum ForegroundTaskState
        {
            Stopped,
            Running,
            Unknown
        }

        private SystemMediaTransportControls systemMediaTransportControls;
        private BackgroundTaskDeferral backgroundTaskDefferal;
        private AutoResetEvent backgroundTaskStarted = new AutoResetEvent(false);
        private BackgroundTaskState backgroundTaskState = BackgroundTaskState.Stopped;
        private ForegroundTaskState foregroundTaskState = ForegroundTaskState.Unknown;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Logger.Current.Init(LogType.BG);

            PlayQueueManager.Current.Connect();

            systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
            systemMediaTransportControls.ButtonPressed += HandleSystemMediaTransportControlsButtonPressed;
            systemMediaTransportControls.PropertyChanged += HandleSystemMediaTransportControlsPropertyChanged;
            systemMediaTransportControls.IsEnabled = true;
            systemMediaTransportControls.IsPauseEnabled = true;
            systemMediaTransportControls.IsPlayEnabled = true;
            systemMediaTransportControls.IsNextEnabled = true;
            systemMediaTransportControls.IsPreviousEnabled = true;

            taskInstance.Canceled += HandleTaskInstanceCanceled;
            taskInstance.Task.Completed += HandleTaskInstanceTaskCompleted;
            taskInstance.Task.Progress += Task_Progress;

            BackgroundMediaPlayer.Current.CurrentStateChanged += HandleBackgroundMediaPlayerCurrentStateChanged;
            BackgroundMediaPlayer.Current.SeekCompleted += HandleBackgroundMediaPlayerSeekComplete;
            BackgroundMediaPlayer.MessageReceivedFromForeground += HandleBackgroundMediaPlayerMessageReceivedFromForeground;

            backgroundTaskStarted.Set();
            backgroundTaskState = BackgroundTaskState.Running;

            backgroundTaskDefferal = taskInstance.GetDeferral();

            PlayQueueManager.Current.TrackChanged += HandlePlayQueueTrackChanged;

            ApplicationSettings.PutSettingsValue(ApplicationSettings.IS_BACKGROUND_PROCESS_ACTIVE, true);

            if (ApplicationSettings.GetSettingsValue<bool>(ApplicationSettings.IS_FOREGROUND_PROCESS_ACTIVE, false))
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Sending message to the FG -- BackgroundStarted");

                PlayQueueManager.Current.SendMessageToForeground(PlayQueueConstantBGMessageId.BackgroundStarted);
                foregroundTaskState = ForegroundTaskState.Running;
            }
            else
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Didn't send message to the FG because FG not started");

                foregroundTaskState = ForegroundTaskState.Stopped;
            }
        }

        void Task_Progress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args)
        {
            
        }

        #region Background Task Stuff

        private void HandleTaskInstanceTaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "AudioPlayer Background Task Completed id:{0}", sender.TaskId);

            backgroundTaskDefferal.Complete();
        }

        private void HandleTaskInstanceCanceled(IBackgroundTaskInstance taskInstance, BackgroundTaskCancellationReason reason)
        {
            PlayQueueManager.Current.Disconnect();

            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "AudioPlayer Background Task Completed id:{0} reason:{1}", taskInstance.Task.TaskId, reason.ToString());
            
            ApplicationSettings.PutSettingsValue(ApplicationSettings.IS_BACKGROUND_PROCESS_ACTIVE, false);

            if (ApplicationSettings.GetSettingsValue<bool>(ApplicationSettings.IS_FOREGROUND_PROCESS_ACTIVE, false))
            {
                PlayQueueManager.Current.SendMessageToForeground(PlayQueueConstantBGMessageId.BackgroundEnded);
            }

            backgroundTaskState = BackgroundTaskState.Stopped;

            PlayQueueManager.Current.TrackChanged -= HandlePlayQueueTrackChanged;

            BackgroundMediaPlayer.Current.CurrentStateChanged -= HandleBackgroundMediaPlayerCurrentStateChanged;
            taskInstance.Task.Completed -= HandleTaskInstanceTaskCompleted;
            taskInstance.Canceled -= HandleTaskInstanceCanceled;

            BackgroundMediaPlayer.Shutdown();

            Logger.Current.Flush();

            backgroundTaskDefferal.Complete();
        }

        #endregion

        #region Media Control Stuff

        private void HandleBackgroundMediaPlayerSeekComplete(MediaPlayer sender, object args)
        {
            PlayQueueManager.Current.SendMessageToForeground(PlayQueueConstantBGMessageId.SeekComplete);
        }

        private async void HandlePlayQueueTrackChanged(PlayQueueManager sender, TrackInfo args)
        {
            int newRowId = 0;

            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Track changed, args set? {0}", args != null);

            if (args != null)
            {
                systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;

                systemMediaTransportControls.DisplayUpdater.Type = MediaPlaybackType.Music;
                systemMediaTransportControls.DisplayUpdater.MusicProperties.Title = args.Title;
                systemMediaTransportControls.DisplayUpdater.MusicProperties.Artist = args.Artist;
                systemMediaTransportControls.DisplayUpdater.MusicProperties.AlbumArtist = args.AlbumArtist;

                systemMediaTransportControls.IsNextEnabled = (args.NextId > 0);
                systemMediaTransportControls.IsPreviousEnabled = (args.PrevId > 0);

                newRowId = args.RowId;
            }
            else
            {
                systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
            }

            if (foregroundTaskState == ForegroundTaskState.Running)
            {
                Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Alerting FG of track change {0}", newRowId);

                PlayQueueManager.Current.SendMessageToForeground(PlayQueueConstantBGMessageId.TrackChanged, newRowId);
            }

            systemMediaTransportControls.DisplayUpdater.Update();

            await UpdateTile(args);
        }

        private async Task UpdateTile(TrackInfo args)
        {
            if (args == null)
            {
                TileUpdateManager.CreateTileUpdaterForApplication("App").Clear();
            }
            else
            {
                string artPath = string.Empty;
                if (!string.IsNullOrEmpty(args.ArtPath))
                {

                    IStorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(args.ArtPath);

                    artPath = file.Path;
                }

                XmlDocument tileTemplate = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText01);

                XmlNodeList textAttributes = tileTemplate.GetElementsByTagName("text");

                textAttributes[0].AppendChild(tileTemplate.CreateTextNode(args.Title));
                textAttributes[1].AppendChild(tileTemplate.CreateTextNode(args.Artist));
                textAttributes[2].AppendChild(tileTemplate.CreateTextNode(args.Album));

                XmlNodeList imageAttribute = tileTemplate.GetElementsByTagName("image");
                XmlElement imageElement = (XmlElement)imageAttribute[0];
                imageElement.SetAttribute("src", artPath);

                // Create the notification from the XML.
                var tileNotification = new TileNotification(tileTemplate);

                try
                {
                    TileUpdateManager.CreateTileUpdaterForApplication("App").Update(tileNotification);
                }
                catch (Exception)
                {
                }
            }

        }

        private void HandleSystemMediaTransportControlsPropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            if (sender.SoundLevel == SoundLevel.Muted)
            {
                BackgroundMediaPlayer.Current.Pause();
            }
        }

        private void HandleSystemMediaTransportControlsButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Play button pressed");

                    if (backgroundTaskState != BackgroundTaskState.Running)
                    {
                        bool success = backgroundTaskStarted.WaitOne(5000);
                        if (!success)
                        {
                            throw new Exception("BackgroundProccessLaunchFailed");
                        }
                    }

                    StartPlayback();

                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Pause button pressed");

                    BackgroundMediaPlayer.Current.Pause();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Next button pressed");

                    SkipToNext();

                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Previous button pressed");
                    SkipToPrevious();

                    break;
                default:
                    Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Unexpected SMTC Button Pressed: {0}", args.Button.ToString());
                    break;
            }
        }

        private void SkipToNext()
        {
            PlayQueueManager.Current.PlayNext();
        }

        private void SkipToPrevious()
        {
            PlayQueueManager.Current.PlayPrev();
        }

        private void StartPlayback()
        {
            if (PlayQueueManager.Current.IsActive)
            {
                BackgroundMediaPlayer.Current.Play();
            }
            else
            {
                PlayQueueManager.Current.Play();
            }
        }

        private void Seek(double percentage)
        {
            if (BackgroundMediaPlayer.Current != null)
            {
                DebugHelper.Assert(new CallerInfo(), 0 <= percentage && percentage <= 1, "Seek to out of ranged position");

                long newTicks = (long)(percentage * BackgroundMediaPlayer.Current.NaturalDuration.Ticks);

                BackgroundMediaPlayer.Current.Position = TimeSpan.FromTicks(newTicks);
            }
            else
            {
                PlayQueueManager.Current.SendMessageToForeground(PlayQueueConstantBGMessageId.SeekComplete); 
                
                DebugHelper.Alert(new CallerInfo(), "Seeking attempt with null BMP");
            }
        }

        private void HandleBackgroundMediaPlayerCurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
        }

        #endregion  

        #region Message Handling

        void HandleBackgroundMediaPlayerMessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key.ToLower())
                {
                    case PlayQueueMessageHelper.AppSuspended:
                        foregroundTaskState = ForegroundTaskState.Stopped;

                        Logger.Current.Log(new CallerInfo(), LogLevel.Info, "GOT SUSPEND EVENT");

                        break;
                    case PlayQueueMessageHelper.AppResumed:
                        foregroundTaskState = ForegroundTaskState.Running;
                        break;
                    case PlayQueueMessageHelper.StartPlayback:
                        PlayQueueManager.Current.Play();
                        break;
                    case PlayQueueMessageHelper.PrevTrack:
                        SkipToPrevious();
                        break;
                    case PlayQueueMessageHelper.SkipTrack:
                        SkipToNext();
                        break;
                    case PlayQueueMessageHelper.PossibleChromeChange:
                        systemMediaTransportControls.IsNextEnabled = PlayQueueManager.Current.CanSkip();
                        systemMediaTransportControls.IsPreviousEnabled = PlayQueueManager.Current.CanBack();
                        break;
                    case PlayQueueMessageHelper.PlayPauseTrack:
                        if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                        {
                            BackgroundMediaPlayer.Current.Pause();
                        }
                        else if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Paused)
                        {
                            BackgroundMediaPlayer.Current.Play();
                        }
                        else
                        {
                            // TODO: #16 flashy flash goes the play button
                            if (backgroundTaskState != BackgroundTaskState.Running)
                            {
                                bool success = backgroundTaskStarted.WaitOne(5000);
                                if (!success)
                                {
                                    throw new Exception("BackgroundProccessLaunchFailed");
                                }
                            }

                            StartPlayback();
                        }
                        break;

                    case PlayQueueMessageHelper.LoggingEnabledChanged:
                        Logger.Current.UpdateEnabled();
                        break;
                    case PlayQueueMessageHelper.ScrubToPercentage:
                        Seek(DebugHelper.CastAndAssert<double>(e.Data[key]));
                        break;
                }
            }
        }

        #endregion
    }
}

using MusicMinkAppLayer.Diagnostics;

namespace MusicMinkAppLayer.PlayQueue
{
    internal enum PlayQueueConstantFGMessageId
    {
        AppSuspended,
        AppResumed,
        StartPlayback,
        LoggingEnabledChanged,
        PlayPauseTrack,
        SkipTrack,
        PrevTrack,
        PossibleChromeChange,
        ScrubToPercentage,
    }

    public enum PlayQueueConstantBGMessageId
    {
        BackgroundStarted,
        BackgroundEnded,
        TrackChanged,
        TrackPlayed,
        SeekComplete,
    }

    /// <summary>
    /// Provides constants to help cleanly send messages across the process boundary
    /// </summary>
    public class PlayQueueMessageHelper
    {
        public const string Unknown = "Unknown";

        #region messages sent from FG

        public const string AppSuspended = "appsuspended";
        public const string AppResumed = "appresumed";

        public const string StartPlayback = "startplayback";

        public const string LoggingEnabledChanged = "loggingchanged";
        public const string PossibleChromeChange = "possiblechromechange";

        public const string PlayPauseTrack = "playpausetrack";
        public const string SkipTrack = "skiptrack";
        public const string PrevTrack = "prevtrack";

        public const string ScrubToPercentage = "scrubtopercentage";

        internal static string FGMessageIdToMessageString(PlayQueueConstantFGMessageId messageId)
        {
            switch (messageId)
            {
                case PlayQueueConstantFGMessageId.AppResumed:
                    return AppResumed;
                case PlayQueueConstantFGMessageId.AppSuspended:
                    return AppSuspended;
                case PlayQueueConstantFGMessageId.LoggingEnabledChanged:
                    return LoggingEnabledChanged;
                case PlayQueueConstantFGMessageId.PlayPauseTrack:
                    return PlayPauseTrack;
                case PlayQueueConstantFGMessageId.PossibleChromeChange:
                    return PossibleChromeChange;
                case PlayQueueConstantFGMessageId.PrevTrack:
                    return PrevTrack;
                case PlayQueueConstantFGMessageId.ScrubToPercentage:
                    return ScrubToPercentage;
                case PlayQueueConstantFGMessageId.SkipTrack:
                    return SkipTrack;
                case PlayQueueConstantFGMessageId.StartPlayback:
                    return StartPlayback;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected PlayQueueConstantFGMessageId {0} encountered", messageId.ToString());
                    return Unknown;
            }
        }

        #endregion

        #region messages sent from BG

        public const string BackgroundStarted = "backgroundstart";
        public const string BackgroundEnded = "backgroundend";

        public const string TrackChanged = "trackchanged";
        public const string TrackPlayed = "trackplayed";
        public const string SeekComplete = "SeekComplete";

        public static string BGMessageIdToMessageString(PlayQueueConstantBGMessageId messageId)
        {
            switch (messageId)
            {
                case PlayQueueConstantBGMessageId.BackgroundEnded:
                    return BackgroundEnded;
                case PlayQueueConstantBGMessageId.BackgroundStarted:
                    return BackgroundStarted;
                case PlayQueueConstantBGMessageId.SeekComplete:
                    return SeekComplete;
                case PlayQueueConstantBGMessageId.TrackChanged:
                    return TrackChanged;
                case PlayQueueConstantBGMessageId.TrackPlayed:
                    return TrackPlayed;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected PlayQueueConstantBGMessageId {0} encountered", messageId.ToString());
                    return Unknown;
            }
        }

        #endregion
    }
}

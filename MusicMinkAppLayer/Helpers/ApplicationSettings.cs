using MusicMinkAppLayer.Diagnostics;
using Windows.Storage;

namespace MusicMinkAppLayer.Helpers
{
    /// <summary>
    /// Helper for accessing values from ApplicationData
    /// </summary>
    public static class ApplicationSettings
    {
        #region KEYS

        public const string SETTING_IS_LOGGING_ENABLED = "SETTING_IS_LOGGING_ENABLED_KEY";

        public const string SETTING_IS_LASTFM_SCROBBLING_ENABLED = "SETTING_IS_LASTFM_SCROBBLING_KEY";
        public const string SETTING_LASTFM_USERNAME = "SETTING_LASTFM_USERNAME_KEY";
        public const string SETTING_LASTFM_PASSWORD = "SETTING_LASTFM_PASSWORD_KEY";
        public const string SETTING_LASTFM_SESSION_TOKEN = "SETTING_LASTFM_SESSION_TOKEN";

        public const string IS_BACKGROUND_PROCESS_ACTIVE = "IS_BACKGROUND_PROCESS_ACTIVE_KEY";
        public const string IS_FOREGROUND_PROCESS_ACTIVE = "IS_FOREGROUND_PROCESS_ACTIVE_KEY";

        public const string CURRENT_PLAYQUEUE_POSITION = "CURRENT_PLAYQUEUE_POSITION_KEY";
        public const string CURRENT_TRACK_PERCENTAGE = "CURRENT_TRACK_PERCENTAGE_KEY";

        #endregion

        public static T GetSettingsValue<T>(string key, T defaultValue)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, defaultValue);

                Logger.Current.Log(new CallerInfo(), LogLevel.Warning, "Setting {0} does not exist, defaulting to {1}", key, defaultValue);
            }

            return DebugHelper.CastAndAssert<T>(ApplicationData.Current.LocalSettings.Values[key]);
        }

        public static void PutSettingsValue(string key, object value)
        {
            Logger.Current.Log(new CallerInfo(), LogLevel.Info, "Setting ApplicationSettingKey {0} to {1}", key, value);
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }
    }
}

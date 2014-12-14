using MusicMink.Common;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel.Email;
using Windows.Storage;

namespace MusicMink.ViewModels
{
    class SettingsViewModel : NotifyPropertyChangedUI
    {
        public const string EMAIL_TARGET = "chimewp@gmail.com";
        public const string EMAIL_NAME = "MusicMink";

        public static class Properties
        {
            public const string AutoPullArtFromLastFM = "AutoPullArtFromLastFM";

            public const string IsLoggingEnabled = "IsLoggingEnabled";
            public const string IsLastFMScrobblingEnabled = "IsLastFMScrobblingEnabled";

            public const string LastFMUsername = "LastFMUsername";
            public const string LastFMPassword = "LastFMPassword";
            public const string LastFMPasswordMask = "LastFMPasswordMask";

            public const string IsLastFMAuthed = "IsLastFMAuthed";
            public const string IsLastFMAuthInProgress = "IsLastFMAuthInProgress";

            public const string IsClassicModeEnabled = "IsClassicModeEnabled";
        }

        private static SettingsViewModel _current;
        public static SettingsViewModel Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new SettingsViewModel();
                }

                return _current;
            }
        }

        private SettingsViewModel()
        {
            this.PropertyChanged += HandleSettingsViewModelPropertyChanged;
        }

        #region Event Handlers

        private void HandleSettingsViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case Properties.IsLoggingEnabled:
                    UploadLogs.RaiseExecuteChanged();
                    Logger.Current.UpdateEnabled();                    
                    break;
                case Properties.LastFMPassword:
                    NotifyPropertyChanged(Properties.LastFMPasswordMask);
                    break;
            }
        }

        #endregion

        #region Commands

        private RelayCommand _uploadLogs;
        public RelayCommand UploadLogs
        {
            get
            {
                if (_uploadLogs == null) _uploadLogs = new RelayCommand(CanExecuteUploadLogs, ExecuteUploadLogs);

                return _uploadLogs;
            }
        }

        private async void ExecuteUploadLogs(object parameter)
        {
            await Logger.Current.Flush();

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            StorageFolder logFolder = await localFolder.GetFolderAsync(Logger.LOG_FOLDER);

            if (logFolder == null) return;

            IReadOnlyList<StorageFile> files = await logFolder.GetFilesAsync();

            EmailRecipient sendTo = new EmailRecipient(EMAIL_TARGET, EMAIL_NAME);

            EmailMessage mail = new EmailMessage();
            mail.Subject = Strings.GetResource("SendLogsSubject");
            mail.Body = Strings.GetResource("SendLogsBody");

            mail.To.Add(sendTo);

            foreach (IStorageFile file in files)
            {
                EmailAttachment logs = new EmailAttachment(file.Name, file);

                mail.Attachments.Add(logs);
            }

            await EmailManager.ShowComposeNewEmailAsync(mail);
        }

        private bool CanExecuteUploadLogs(object parameter)
        {
            return IsLoggingEnabled;
        }

        #endregion

        #region Properties

        public bool AutoPullArtFromLastFM
        {
            get
            {
                return GetSettingField<bool>(ApplicationSettings.SETTING_IS_AUTO_PULL_ART_FROM_LASTFM_ON, true);
            }
            set
            {
                SetSettingField<bool>(ApplicationSettings.SETTING_IS_AUTO_PULL_ART_FROM_LASTFM_ON, value, Properties.AutoPullArtFromLastFM);
            }
        }

        public bool IsLoggingEnabled
        {
            get
            {
                return GetSettingField<bool>(ApplicationSettings.SETTING_IS_LOGGING_ENABLED, true);
            }
            set
            {
                SetSettingField<bool>(ApplicationSettings.SETTING_IS_LOGGING_ENABLED, value, Properties.IsLoggingEnabled);
            }
        }

        public bool IsLastFMScrobblingEnabled
        {
            get
            {
                return GetSettingField<bool>(ApplicationSettings.SETTING_IS_LASTFM_SCROBBLING_ENABLED, false);
            }
            set
            {
                SetSettingField<bool>(ApplicationSettings.SETTING_IS_LASTFM_SCROBBLING_ENABLED, value, Properties.IsLastFMScrobblingEnabled);
            }
        }

        public string LastFMUsername
        {
            get
            {
                return GetSettingField<string>(ApplicationSettings.SETTING_LASTFM_USERNAME, string.Empty);
            }
            set
            {
                SetSettingField<string>(ApplicationSettings.SETTING_LASTFM_USERNAME, value, Properties.LastFMUsername);
            }
        }

        public string LastFMPassword
        {
            get
            {
                return GetSettingField<string>(ApplicationSettings.SETTING_LASTFM_PASSWORD, string.Empty);
            }
            set
            {
                SetSettingField<string>(ApplicationSettings.SETTING_LASTFM_PASSWORD, value, Properties.LastFMPassword);
            }
        }

        public string LastFMPasswordMask
        {
            get
            {
                if (string.IsNullOrEmpty(LastFMPassword)) return string.Empty;

                else return "          ";
            }
            set
            {
                SetSettingField<string>(ApplicationSettings.SETTING_LASTFM_PASSWORD, value, Properties.LastFMPassword);
            }
        }
        
        public bool IsLastFMAuthed
        {
            get
            {
                return !(string.IsNullOrEmpty(GetSettingField<string>(ApplicationSettings.SETTING_LASTFM_SESSION_TOKEN, string.Empty)));
            }
        }

        private bool _isLastFMAuthInProgress = false;
        public bool IsLastFMAuthInProgress
        {
            get
            {
                return _isLastFMAuthInProgress;
            }
            set
            {
                if (_isLastFMAuthInProgress != value)
                {
                    _isLastFMAuthInProgress = value;
                    NotifyPropertyChanged(Properties.IsLastFMAuthInProgress);
                }
            }
        }

        public bool IsClassicModeEnabled
        {
            get
            {
                return GetSettingField<bool>(ApplicationSettings.SETTING_IS_CLASSIC_MODE_ON, false);
            }
            set
            {
                SetSettingField<bool>(ApplicationSettings.SETTING_IS_CLASSIC_MODE_ON, value, Properties.IsClassicModeEnabled);
            }
        }

        #endregion

        #region Properties

        public async void ReauthLastFM()
        {
            IsLastFMAuthInProgress = true;
            await LastFMManager.Current.UpdateScrobbleSession();
            IsLastFMAuthInProgress = false;

            NotifyPropertyChanged(Properties.IsLastFMAuthed);
        }

        #endregion

        #region ReflectionHelpers

        protected void SetSettingField<U>(string setting_key, U value, string property)
        {
            U currentValue = ApplicationSettings.GetSettingsValue<U>(setting_key, default(U));

            if (!EqualityComparer<U>.Default.Equals(currentValue, value))
            {
                ApplicationSettings.PutSettingsValue(setting_key, value);
                NotifyPropertyChanged(property);
            }
        }

        protected U GetSettingField<U>(string setting_key, U defaultValue)
        {
            return ApplicationSettings.GetSettingsValue<U>(setting_key, defaultValue);
        }

        #endregion
    }
}

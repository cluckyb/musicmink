using MusicMink.ViewModels;
using System;
using Windows.UI.Xaml;

namespace MusicMink.Pages
{
    /// <summary>
    /// Settings!
    /// </summary>
    public sealed partial class Settings : BasePage
    {
        public Settings()
        {
            this.InitializeComponent();

            this.DataContext = SettingsViewModel.Current;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SettingsViewModel.Current.LastFMUsername = LastFMUsernameTextBox.Text;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LastFMPasswordBox.Password = string.Empty;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SettingsViewModel.Current.LastFMPassword = LastFMPasswordBox.Password;
            SettingsViewModel.Current.ReauthLastFM();
        }
    }
}

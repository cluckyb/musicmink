using MusicMink.ViewModels;
using Windows.UI.Xaml.Input;

namespace MusicMink.Pages
{
    /// <summary>
    /// Root hub for accessing all the other pages
    /// </summary>
    public sealed partial class HomePage : BasePage
    {
        public HomePage()
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current;
        }

        private void HandleSongsNavigationItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.SongList);
        }

        private void HandleLibraryNavigationItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.ManageLibrary);
        }

        private void HandleAlbumNavigationItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.AlbumList);
        }

        private void HandleSettingsNavigationItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.SettingsPage);
        }

        private void HandlePlaylistNavigationItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.PlaylistList);
        }

        private void HandleArtistsNavigationItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.ArtistList);
        }

        private void HandleSearchNavigationItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.SearchPage);
        }

        private void HandleMixesNavigationItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.MixList);
        }
    }
}

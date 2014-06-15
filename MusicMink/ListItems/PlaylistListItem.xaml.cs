using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MusicMink.ListItems
{
    public sealed partial class PlaylistListItem : UserControl
    {
        private PlaylistViewModel Playlist
        {
            get
            {
                return DebugHelper.CastAndAssert<PlaylistViewModel>(this.DataContext);
            }
        }

        public PlaylistListItem()
        {
            this.InitializeComponent();
        }

        private void HandlePlaylistItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.PlaylistPage, Playlist.PlaylistId);
        }
    }
}

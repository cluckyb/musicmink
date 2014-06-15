using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MusicMink.Dialogs
{
    public sealed partial class EditPlaylist : ContentDialog
    {
        private PlaylistViewModel Playlist
        {
            get
            {
                return DebugHelper.CastAndAssert<PlaylistViewModel>(this.DataContext);
            }
        }

        public bool WasDelete = false;

        public EditPlaylist(PlaylistViewModel playlistToEdit)        
        {
            this.InitializeComponent();

            this.DataContext = playlistToEdit;
        }

        private void HandleContentDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if ((deletePlaylist.IsChecked.HasValue && deletePlaylist.IsChecked.Value) &&
                (deletePlaylistConfirm.IsChecked.HasValue && deletePlaylistConfirm.IsChecked.Value))
            {
                WasDelete = true;

                LibraryViewModel.Current.DeletePlaylist(Playlist);
            }
            else
            {
                Playlist.Name = newName.Text;
            }
        }

        private void HandleContentDialogSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void HandleDeletePlaylistChecked(object sender, RoutedEventArgs e)
        {
            deletePlaylistConfirm.Visibility = Visibility.Visible;
        }

        private void HandleDeletePlaylistUnchecked(object sender, RoutedEventArgs e)
        {
            deletePlaylistConfirm.IsChecked = false;
            deletePlaylistConfirm.Visibility = Visibility.Collapsed;
        }
    }
}

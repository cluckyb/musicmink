using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace MusicMink.Dialogs
{
    public sealed partial class AddToPlaylist : ContentDialog
    {
        private SongViewModel Song;

        public int selectedPlaylistId = -1;

        public AddToPlaylist(SongViewModel songToAdd)
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current;

            Song = songToAdd;
        }

        private void HandleContentDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var selectedItem = AddComboBox.SelectedItem;

            if (selectedItem == null) return;

            PlaylistViewModel selectedPlaylist = DebugHelper.CastAndAssert<PlaylistViewModel>(selectedItem);

            selectedPlaylistId = selectedPlaylist.PlaylistId;

            selectedPlaylist.AddSong(Song);
        }

        private void HandleContentDialogSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}

using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MusicMink.Dialogs
{
    public sealed partial class EditSong : ContentDialog
    {
        private SongViewModel Song
        {
            get
            {
                return DebugHelper.CastAndAssert<SongViewModel>(this.DataContext);
            }
        }

        public EditSong(SongViewModel songToEdit)
        {
            this.InitializeComponent();

            this.DataContext = songToEdit;
        }

        private void HandleContentDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if ((deleteSong.IsChecked.HasValue && deleteSong.IsChecked.Value) &&
                (deleteSongConfirm.IsChecked.HasValue && deleteSongConfirm.IsChecked.Value))
            {
                LibraryViewModel.Current.DeleteSong(Song);
            }
            else
            {
                Song.Name = editSongName.Text;

                Song.ArtistName = editArtistName.Text;

                string newAlbumName = editAlbumName.Text;
                string newAlbumAristName = editAlbumAritstName.Text;

                if (newAlbumName != Song.Album.Name || newAlbumAristName != Song.Album.ArtistName)
                {
                    ArtistViewModel albumArtistViewModel = LibraryViewModel.Current.LookupArtistByName(newAlbumAristName);
                    AlbumViewModel newAlbumViewModel = LibraryViewModel.Current.LookupAlbumByName(newAlbumName, albumArtistViewModel.ArtistId);

                    Song.UpdateAlbum(newAlbumViewModel);
                }

                uint newTrackNumber;

                if (uint.TryParse(editTrackNumber.Text, out newTrackNumber))
                {
                    Song.TrackNumber = newTrackNumber;
                }
            }
        }

        private void HandleContentDialogSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void HandleLowerTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            Transform stackPanelRestore = DebugHelper.CastAndAssert<Transform>(((TextBox)sender).TransformToVisual(rootStackPanel));

            TranslateTransform shiftDown = new TranslateTransform();
            shiftDown.Y = 90;

            TransformGroup group = new TransformGroup();

            group.Children.Add(DebugHelper.CastAndAssert<Transform>(stackPanelRestore.Inverse));
            group.Children.Add(shiftDown);

            rootStackPanel.RenderTransform = group;
        }

        private void HandleLowerTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            rootStackPanel.RenderTransform = null;
        }

        private void HandleDeleteSongChecked(object sender, RoutedEventArgs e)
        {
            deleteSongConfirm.Visibility = Visibility.Visible;
        }

        private void HandleDeleteSongUnchecked(object sender, RoutedEventArgs e)
        {
            deleteSongConfirm.IsChecked = false;
            deleteSongConfirm.Visibility = Visibility.Collapsed;
        }
    }
}

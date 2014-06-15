using MusicMink.ViewModels;
using Windows.UI.Xaml.Controls;

namespace MusicMink.Dialogs
{
    public sealed partial class AddPlaylist : ContentDialog
    {
        public AddPlaylist()
        {
            this.InitializeComponent();
        }

        private void HandleContentDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            LibraryViewModel.Current.AddPlaylist(newName.Text);            
        }

        private void HandleContentDialogSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            return;
        }
    }
}

using MusicMink.ViewModels;
using Windows.UI.Xaml.Controls;

namespace MusicMink.Dialogs
{
    public sealed partial class AddMix : ContentDialog
    {
        public AddMix()
        {
            this.InitializeComponent();
        }

        private void HandleContentDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            LibraryViewModel.Current.AddMix(newName.Text);            
        }

        private void HandleContentDialogSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            return;
        }
    }
}

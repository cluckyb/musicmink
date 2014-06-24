using MusicMink.Dialogs;
using MusicMink.ViewModels;
using Windows.UI.Xaml;

namespace MusicMink.Pages
{
    /// <summary>
    /// List of all mixes
    /// </summary>
    public sealed partial class MixList : BasePage
    {
        public MixList()
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current;
        }
    }
}

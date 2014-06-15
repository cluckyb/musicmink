using MusicMink.Dialogs;
using MusicMink.ViewModels;
using Windows.UI.Xaml;

namespace MusicMink.Pages
{
    /// <summary>
    /// List of all playlists
    /// </summary>
    public sealed partial class PlaylistList : BasePage
    {
        public PlaylistList()
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current;
        }
    }
}

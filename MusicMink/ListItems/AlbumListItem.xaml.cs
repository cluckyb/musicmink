using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MusicMink.ListItems
{
    public sealed partial class AlbumListItem : UserControl
    {
        private AlbumViewModel Album
        {
            get
            {
                return DebugHelper.CastAndAssert<AlbumViewModel>(this.DataContext);
            }
        }

        public AlbumListItem()
        {
            this.InitializeComponent();
        }

        void HandleAlbumListItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.AlbumPage, Album.AlbumId);
        }
    }
}

using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MusicMink.ListItems
{
    public sealed partial class ArtistListItem : UserControl
    {
        private ArtistViewModel Artist
        {
            get
            {
                return DebugHelper.CastAndAssert<ArtistViewModel>(this.DataContext);
            }
        }

        public ArtistListItem()
        {
            this.InitializeComponent();
        }

        void HandleArtistListItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.ArtistPage, Artist.ArtistId);
        }

    }
}

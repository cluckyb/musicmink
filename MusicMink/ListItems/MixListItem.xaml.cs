using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MusicMink.ListItems
{
    public sealed partial class MixListItem : UserControl
    {
        private MixViewModel Mix
        {
            get
            {
                return DebugHelper.CastAndAssert<MixViewModel>(this.DataContext);
            }
        }

        public MixListItem()
        {
            this.InitializeComponent();
        }

        private void HandleMixItemTapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.MixPage, Mix.MixId);
        }
    }
}

using MusicMink.ViewModels;
using System;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MusicMink.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlaying : BasePage
    {
        public NowPlaying()
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current.PlayQueue;
        }

        private void PrevTrackImageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (LibraryViewModel.Current.PlayQueue.PrevPlayer.CanExecute(null))
            {
                LibraryViewModel.Current.PlayQueue.PrevPlayer.Execute(null);
            }           
        }

        private void NextTrackImageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (LibraryViewModel.Current.PlayQueue.SkipPlayer.CanExecute(null))
            {
                LibraryViewModel.Current.PlayQueue.SkipPlayer.Execute(null);
            }
        }

        private void Rectangle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (Math.Abs(e.Cumulative.Translation.Y) < 10)
            {
                if (e.Cumulative.Translation.X > 30 && LibraryViewModel.Current.PlayQueue.PrevPlayer.CanExecute(null))
                {
                    LibraryViewModel.Current.PlayQueue.PrevPlayer.Execute(null);
                }
                else if (e.Cumulative.Translation.X < -30 && LibraryViewModel.Current.PlayQueue.SkipPlayer.CanExecute(null))
                {
                    LibraryViewModel.Current.PlayQueue.SkipPlayer.Execute(null);
                }
            }
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationManager.Current.Navigate(NavigationLocation.Queue);
        }
    }
}

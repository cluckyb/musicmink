using MusicMink.ListItems;
using MusicMink.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MusicMink.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Queue : BasePage
    {
        public Queue()
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current.PlayQueue;

            this.PlayqueueList.Loaded += HandlePlayqueueListLoaded;
        }

        void HandlePlayqueueListLoaded(object sender, RoutedEventArgs e)
        {
            if (LibraryViewModel.Current.PlayQueue.CurrentTrackPosition > 3)
            {
                PlayQueueEntryViewModel pqeVM = LibraryViewModel.Current.PlayQueue.PlaybackQueue[LibraryViewModel.Current.PlayQueue.CurrentTrackPosition - 3];

                PlayqueueList.ScrollIntoView(pqeVM, ScrollIntoViewAlignment.Leading);
            }
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

        private void HandlePlayqueueListHolding(object sender, HoldingRoutedEventArgs e)
        {
            PlayQueueListItem.CloseExpandedEntry();

            PlayqueueList.ReorderMode = ListViewReorderMode.Enabled;
        }

        private void HandlePlayqueueListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PlayqueueList.ReorderMode = ListViewReorderMode.Disabled;
        }

        private void HandlePlayQueueReorderAppBarToggleButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            PlayQueueListItem.CloseExpandedEntry();
        }
    }
}

using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MusicMink.ListItems
{
    public sealed partial class PlayQueueListItem : UserControl
    {
        public static PlayQueueListItem currentExpandedEntry;

        private SongViewModel Song
        {
            get
            {
                return DebugHelper.CastAndAssert<SongViewModel>(this.DataContext);
            }
        }

        private ListView parentList;
        private ListView ParentList
        {
            get
            {
                if (parentList == null)
                {
                    DependencyObject element = this;

                    while ((element != null) && !(element is ListView))
                    {
                        element = VisualTreeHelper.GetParent(element);
                    }

                    DebugHelper.Assert(new CallerInfo(), element != null, "PlayQueueListItem member of a ListView");

                    parentList = (ListView) element;
                }

                return parentList;
            }
        }

        public PlayQueueListItem()
        {
            this.InitializeComponent();
        }

        private void HandleHeaderTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ParentList != null && ParentList.ReorderMode == ListViewReorderMode.Enabled) return;

            if (currentExpandedEntry != this)
            {
                if (currentExpandedEntry != null)
                {
                    VisualStateManager.GoToState(currentExpandedEntry, "ExpandedStateClosed", true);
                }

                currentExpandedEntry = this;
                VisualStateManager.GoToState(this, "ExpandedStateOpen", true);
            }
            else
            {
                VisualStateManager.GoToState(currentExpandedEntry, "ExpandedStateClosed", true);
                currentExpandedEntry = null;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            LibraryViewModel.Current.PlayQueue.PlaySong(Song);
        }

        private void QueueButton_Click(object sender, RoutedEventArgs e)
        {
            LibraryViewModel.Current.PlayQueue.QueueSong(Song);
        }

        private void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            // PopupManager.Current.OpenPopup(new AddToPlaylist((SongViewModel)this.DataContext));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // PopupManager.Current.OpenPopup(new EditSong((SongViewModel)this.DataContext));
        }

        internal static void CloseExpandedEntry()
        {
            if (currentExpandedEntry != null)
            {
                VisualStateManager.GoToState(currentExpandedEntry, "ExpandedStateClosed", true);
                currentExpandedEntry = null;
            }
        }

        private void TextBlock_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double newOffset = NameViewer.HorizontalOffset - 3 * e.Delta.Translation.X;

            NameViewer.ChangeView(newOffset, null, null);
        }
    }
}

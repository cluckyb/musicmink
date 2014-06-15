using MusicMink.Dialogs;
using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MusicMink.ListItems
{
    public enum SongListItemView
    {
        SongList,
        AlbumPage,
        PlayQueue
    }

    public sealed partial class SongListItem : UserControl
    {
        public static readonly DependencyProperty ViewTypeProperty =
            DependencyProperty.Register(
            "ViewType", typeof(SongListItemView),
            typeof(SongListItem), new PropertyMetadata(SongListItemView.SongList, OnViewTypePropertyChanged)
            );

        private static void OnViewTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SongListItem songListItem = DebugHelper.CastAndAssert<SongListItem>(d);

            songListItem.UpdateView();
        }

        public SongListItemView ViewType
        {
            get { return (SongListItemView)GetValue(ViewTypeProperty); }
            set { SetValue(ViewTypeProperty, value); }
        }

        public static SongListItem currentExpandedEntry;

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

                    parentList = (ListView)element;
                }

                return parentList;
            }
        }


        public SongListItem()
        {
            this.InitializeComponent();
        }

        private void HandleStackPanelTapped(object sender, TappedRoutedEventArgs e)
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

        private void UpdateView()
        {
            switch (this.ViewType)
            {
                case SongListItemView.SongList:
                    VisualStateManager.GoToState(this, "ViewTypeSongList", false);
                    break;
                case SongListItemView.AlbumPage:
                    VisualStateManager.GoToState(this, "ViewTypeAlbumPage", false);
                    break;
                case SongListItemView.PlayQueue:
                    VisualStateManager.GoToState(this, "ViewTypePlayQueue", false);
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unknown SongListItemView {0}", this.ViewType);
                    break;
            }
        }

        internal static void CloseExpandedEntry()
        {
            if (currentExpandedEntry != null)
            {
                VisualStateManager.GoToState(currentExpandedEntry, "ExpandedStateClosed", true);
                currentExpandedEntry = null;
            }
        }

        private void HandleNameTextBlockManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double newOffset = NameViewer.HorizontalOffset - 3 * e.Delta.Translation.X;

            NameViewer.ChangeView(newOffset, null, null);
        }
    }
}

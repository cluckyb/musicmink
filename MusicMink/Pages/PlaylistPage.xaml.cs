using MusicMink.Dialogs;
using MusicMink.ListItems;
using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace MusicMink.Pages
{
    /// <summary>
    /// Details about a signle playlist
    /// </summary>
    public sealed partial class PlaylistPage : BasePage
    {
        private PlaylistViewModel Playlist
        {
            get
            {
                return DebugHelper.CastAndAssert<PlaylistViewModel>(this.DataContext);
            }
        }

        public PlaylistPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.DataContext = LibraryViewModel.Current.LookupPlaylistById((int)e.Parameter);

            if (Playlist == null)
            {
                Frame.GoBack();
            }
            else
            {
                Playlist.PropertyChanged += HandlePlaylistPropertyChanged;
            }
        }

        #region Event Handlers

        void HandlePlaylistPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case PlaylistViewModel.Properties.IsBeingDeleted:
                    Frame.GoBack();
                    break;
            }
        }

        private void HandlePlaylistHolding(object sender, HoldingRoutedEventArgs e)
        {
            SongListItem.CloseExpandedEntry();

            PlaylistListView.ReorderMode = ListViewReorderMode.Enabled;
        }

        private void HandlePlaylistDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PlaylistListView.ReorderMode = ListViewReorderMode.Disabled;
        }

        private void HandlePlaylistReorderToggleButtonClick(object sender, RoutedEventArgs e)
        {
            SongListItem.CloseExpandedEntry();
        }
        
        #endregion
    }
}

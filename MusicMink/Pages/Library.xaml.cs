using MusicMink.Common;
using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml;

namespace MusicMink.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Library : BasePage
    {
        private ListRestoreHelper songListRestoreHelper;
        private ListRestoreHelper albumListRestoreHelper;
        private ListRestoreHelper artistListRestoreHelper;

        public Library()
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current;

            songListRestoreHelper = new ListRestoreHelper(SongsZoomedInListView, "SONGS");
            albumListRestoreHelper = new ListRestoreHelper(AlbumsZoomedInView, "ALBUMS");
            artistListRestoreHelper = new ListRestoreHelper(ArtistsZoomedInListView, "ARTISTS");

            this.SongListRoot.Loaded += HandleSongListRootLoaded;
            this.AlbumListRoot.Loaded += HandleAlbumListRootLoaded;
            this.ArtistListRoot.Loaded += HandleArtistListRootLoaded;
        }

        void HandleSongListRootLoaded(object sender, RoutedEventArgs e)
        {
            songListRestoreHelper.ScrollSavedItemIntoView();
        }

        void HandleAlbumListRootLoaded(object sender, RoutedEventArgs e)
        {
            albumListRestoreHelper.ScrollSavedItemIntoView();
        }

        void HandleArtistListRootLoaded(object sender, RoutedEventArgs e)
        {
            artistListRestoreHelper.ScrollSavedItemIntoView();
        }

        protected override void HandleNavigationHelperLoadState(object sender, LoadStateEventArgs e)
        {
            base.HandleNavigationHelperLoadState(sender, e);

            object lastIndex;

            if (e.PageState != null && e.PageState.TryGetValue("LIBRARY_PIVOT_INDEX", out lastIndex))
            {
                RootPivot.SelectedIndex = DebugHelper.CastAndAssert<int>(lastIndex);
            }

            songListRestoreHelper.LoadState(e.PageState);
            albumListRestoreHelper.LoadState(e.PageState);
            artistListRestoreHelper.LoadState(e.PageState);
        }

        protected override void HandleNavigationHelperSaveState(object sender, SaveStateEventArgs e)
        {
            base.HandleNavigationHelperSaveState(sender, e);

            e.PageState.Add("LIBRARY_PIVOT_INDEX", RootPivot.SelectedIndex);
            
            songListRestoreHelper.SaveState(e.PageState);
            albumListRestoreHelper.SaveState(e.PageState);
            artistListRestoreHelper.SaveState(e.PageState);
        }
    }
}

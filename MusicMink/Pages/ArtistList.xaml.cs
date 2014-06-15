using MusicMink.Common;
using MusicMink.ViewModels;
using Windows.UI.Xaml;

namespace MusicMink.Pages
{
    /// <summary>
    /// List of Artists, sorted alphabetically with groups for each first letter
    /// </summary>
    public sealed partial class ArtistList : BasePage
    {
        private ListRestoreHelper listRestoreHelper;

        public ArtistList() : base()
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current;

            this.Loaded += HandleArtistListLoaded;

            listRestoreHelper = new ListRestoreHelper(ZoomedInListView);
        }

        void HandleArtistListLoaded(object sender, RoutedEventArgs e)
        {
            listRestoreHelper.ScrollSavedItemIntoView();
        }

        protected override void HandleNavigationHelperLoadState(object sender, LoadStateEventArgs e)
        {
            base.HandleNavigationHelperLoadState(sender, e);

            listRestoreHelper.LoadState(e.PageState);
        }

        protected override void HandleNavigationHelperSaveState(object sender, SaveStateEventArgs e)
        {
            base.HandleNavigationHelperSaveState(sender, e);

            listRestoreHelper.SaveState(e.PageState);
        }
    }
}

using MusicMink.Common;
using MusicMink.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace MusicMink.Pages
{
    /// <summary>
    /// List of all songs in the library
    /// </summary>
    public sealed partial class SongList : BasePage
    {
        private ListRestoreHelper listRestoreHelper;

        public SongList()
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current;

            listRestoreHelper = new ListRestoreHelper(ZoomedInListView);

            this.Loaded += SongList_Loaded;
        }

        void SongList_Loaded(object sender, RoutedEventArgs e)
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

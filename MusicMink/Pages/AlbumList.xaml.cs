using MusicMink.Common;
using MusicMink.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MusicMink.Pages
{
    /// <summary>
    /// Lists all of the albums, grouped by first letter in a grid like format
    /// </summary>
    public sealed partial class AlbumList : BasePage
    {
        private ListRestoreHelper listRestoreHelper;

        public AlbumList() : base()
        {
            this.InitializeComponent();

            this.DataContext = LibraryViewModel.Current;

            listRestoreHelper = new ListRestoreHelper(ZoomedInView);

            this.Loaded += AlbumList_Loaded;
        }

        void AlbumList_Loaded(object sender, RoutedEventArgs e)
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

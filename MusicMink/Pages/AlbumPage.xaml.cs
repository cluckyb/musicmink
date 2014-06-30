using MusicMink.Dialogs;
using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace MusicMink.Pages
{
    /// <summary>
    /// Detailed info about a single album
    /// </summary>
    public sealed partial class AlbumPage : BasePage
    {
        private AlbumViewModel Album
        {
            get
            {
                return DebugHelper.CastAndAssert<AlbumViewModel>(this.DataContext);
            }
        }

        public AlbumPage() : base()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.DataContext = LibraryViewModel.Current.LookupAlbumById((int) e.Parameter);

            if (Album == null)
            {
                Frame.GoBack();
            }
            else
            {
                Album.PropertyChanged += HandleAlbumPropertyChanged;
            }
        }

        #region Event Handlers

        void HandleAlbumPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case AlbumViewModel.Properties.IsBeingDeleted:
                    if (Album.IsBeingDeleted)
                    {
                        Frame.GoBack();
                    }
                    break;
            }
        }

        internal async void HandleFilePickerLaunch(FileOpenPickerContinuationEventArgs filePickerOpenArgs)
        {
            DebugHelper.Assert(new CallerInfo(), filePickerOpenArgs.Files.Count == 1);

            EditAlbum editAlbumDialog = new EditAlbum(Album, filePickerOpenArgs.Files[0]);

            await editAlbumDialog.ShowAsync();
        }

        #endregion
    }
}

using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MusicMink.Pages
{
    /// <summary>
    /// Detailed info about an Artist
    /// </summary>
    public sealed partial class ArtistPage : BasePage
    {
        private ArtistViewModel Artist
        {
            get
            {
                return DebugHelper.CastAndAssert<ArtistViewModel>(this.DataContext);
            }
        }

        public ArtistPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.DataContext = LibraryViewModel.Current.LookupArtistById((int)e.Parameter);

            if (Artist == null)
            {
                Frame.GoBack();
            }
            else
            {
                Artist.PropertyChanged += HandleArtistPropertyChanged;
            }
        }

        void HandleArtistPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case ArtistViewModel.Properties.IsBeingDeleted:
                    Frame.GoBack();
                    break;
            }
        }
    }
}

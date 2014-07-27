using MusicMink.Dialogs;
using MusicMink.ListItems;
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
    /// Details about a signle playlist
    /// </summary>
    public sealed partial class MixPage : BasePage
    {
        private MixViewModel Mix
        {
            get
            {
                return DebugHelper.CastAndAssert<MixViewModel>(this.DataContext);
            }
        }

        public MixPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.DataContext = LibraryViewModel.Current.LookupMixById((int)e.Parameter);

            if (Mix == null)
            {
                Frame.GoBack();
            }
            else
            {
                Mix.PropertyChanged += HandleMixPropertyChanged;
            }
        }

        #region Event Handlers

        void HandleMixPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case MixViewModel.Properties.IsBeingDeleted:
                    if (Mix.IsBeingDeleted)
                    {
                        Frame.GoBack();
                    }
                    break;
            }
        }
        
        #endregion
    }
}

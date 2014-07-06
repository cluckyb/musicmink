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
    public sealed partial class SearchPage : BasePage
    {
        public SearchPage()
            : base()
        {
            this.InitializeComponent();

            this.DataContext = SearchManagerViewModel.Current;
        }

        private void SearchTerm_LostFocus(object sender, RoutedEventArgs e)
        {
            SearchManagerViewModel.Current.Search(SearchTerm.Text);
        }

        #region Event Handlers

        #endregion
    }
}

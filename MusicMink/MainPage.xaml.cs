using MusicMink.ListItems;
using MusicMink.ViewModels;
using System;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace MusicMink
{
    /// <summary>
    /// Root page. Contains the playqueue control, and then a frame that hosts all the other pages
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool isPlayqueueExpanded = false;

        public MainPage()
        {
            this.InitializeComponent();

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            MainContentFrame.ContentTransitions.Clear();
            MainContentFrame.ContentTransitions.Add(new ContentThemeTransition());

            HardwareButtons.BackPressed += HandleHardwareButtonsBackPressed;
        }

        void HandleHardwareButtonsBackPressed(object sender, BackPressedEventArgs e)
        {
            if (isPlayqueueExpanded)
            {
                PlayQueueListItem.CloseExpandedEntry();

                PlayqueueList.ReorderMode = ListViewReorderMode.Disabled;

                VisualStateManager.GoToState(this, "PlayQueueHidden", true);
                isPlayqueueExpanded = false;

                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.DataContext = LibraryViewModel.Current;

            NavigationManager.Current.SetRootFrame(MainContentFrame);

            NavigationManager.Current.Navigate(NavigationLocation.Home);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        #region Playback Control Event Handlers

        private void HandlePullDownArrowContainerClick(object sender, RoutedEventArgs e)
        {
            if (!isPlayqueueExpanded)
            {
                VisualStateManager.GoToState(this, "PlayQueueOut", true);
                isPlayqueueExpanded = true;
                MainContentFrame.IsEnabled = false;
            }
            else
            {
                PlayQueueListItem.CloseExpandedEntry();

                PlayqueueList.ReorderMode = ListViewReorderMode.Disabled;

                VisualStateManager.GoToState(this, "PlayQueueHidden", true);
                isPlayqueueExpanded = false;
            }
        }

        private void HandleHomeContainerClick(object sender, RoutedEventArgs e)
        {
            if (isPlayqueueExpanded)
            {
                PlayQueueListItem.CloseExpandedEntry();

                PlayqueueList.ReorderMode = ListViewReorderMode.Disabled;

                VisualStateManager.GoToState(this, "PlayQueueHidden", true);
                isPlayqueueExpanded = false;
            }

            NavigationManager.Current.GoHome();
        }

        private void PlayQueueHiddenCompleted(object sender, object e)
        {
            MainContentFrame.IsEnabled = true;
        }

        private void PlayQueueOutCompleted(object sender, object e)
        {
            MainContentFrame.IsEnabled = false;
        }

        private void HandlePlayqueueListHolding(object sender, HoldingRoutedEventArgs e)
        {
            PlayQueueListItem.CloseExpandedEntry();

            PlayqueueList.ReorderMode = ListViewReorderMode.Enabled;
        }

        private void HandlePlayqueueListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PlayqueueList.ReorderMode = ListViewReorderMode.Disabled;
        }

        private void HandlePlayQueueReorderAppBarToggleButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            PlayQueueListItem.CloseExpandedEntry();
        }

    #endregion

        #region Bezzel Manipluation

        Binding savedWidthBinding;
        Binding savedTextBinding;
        private void HandlePlayerControlProgressBarBezzelManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            BindingExpression bindingExpression = PlayerControlProgressBarCompleted.GetBindingExpression(Rectangle.WidthProperty);
            savedWidthBinding = bindingExpression.ParentBinding;

            BindingExpression textBindingExpression = ProgressTextBlock.GetBindingExpression(TextBlock.TextProperty);
            savedTextBinding = textBindingExpression.ParentBinding;

            PlayerControlProgressBarCompleted.Width = PlayerControlProgressBarCompleted.ActualWidth;
        }

        private void HandlePlayerControlProgressBarBezzelManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double newWidth = PlayerControlProgressBarCompleted.Width + e.Delta.Translation.X;

            if (newWidth < 0)
            {
                newWidth = 0;
            }
            else if (newWidth > PlayerControlProgressBarFull.Width)
            {
                newWidth = PlayerControlProgressBarFull.Width;
            }

            if (PlayerControlProgressBarFull.Width > 0)
            {
                double percentage = newWidth / PlayerControlProgressBarFull.Width;

                TimeSpan newTime = TimeSpan.FromTicks((long)(percentage * LibraryViewModel.Current.PlayQueue.TotalTicks));

                ProgressTextBlock.Text = newTime.ToString(@"%m\:ss");
            }
            else
            {
                ProgressTextBlock.Text = TimeSpan.Zero.ToString(@"%m\:ss");
            }

            PlayerControlProgressBarCompleted.Width = newWidth;
        }

        private void HandlePlayerControlProgressBarBezzelManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (PlayerControlProgressBarFull.Width > 0)
            {
                double percentage = PlayerControlProgressBarCompleted.Width / PlayerControlProgressBarFull.Width;

                LibraryViewModel.Current.PlayQueue.ScrubToPercentage(percentage);
            }
            else
            {
                LibraryViewModel.Current.PlayQueue.ScrubToPercentage(0);
            }

            PlayerControlProgressBarCompleted.SetBinding(Rectangle.WidthProperty, savedWidthBinding);
            savedWidthBinding = null;

            ProgressTextBlock.SetBinding(TextBlock.TextProperty, savedTextBinding);
            savedTextBinding = null;
        }

        #endregion

    }
}

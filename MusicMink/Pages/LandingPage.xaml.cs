using MusicMink.ViewModels;
using System;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace MusicMink.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingPage : BasePage
    {
        public LandingPage()
        {
            this.InitializeComponent();

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            MainContentFrame.ContentTransitions.Clear();
            MainContentFrame.ContentTransitions.Add(new ContentThemeTransition());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.DataContext = LibraryViewModel.Current.PlayQueue;
            this.HomeButton.DataContext = LibraryViewModel.Current;
            this.CurrentTrackPanel.DataContext = LibraryViewModel.Current;

            NavigationManager.Current.SetRootFrame(MainContentFrame);
            NavigationManager.Current.Navigate(NavigationLocation.NewHome);
        }

        Binding savedWidthBinding;
        private void HandlePlayerControlProgressBarBezzelManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            ProgressBarScrubView.Visibility = Visibility.Visible;

            BindingExpression bindingExpression = PlayerControlProgressBarCompleted.GetBindingExpression(Rectangle.WidthProperty);
            savedWidthBinding = bindingExpression.ParentBinding;
        }

        private void HandlePlayerControlProgressBarBezzelManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double newWidth = e.Position.X;

            if (newWidth < 0) newWidth = 0;
            if (newWidth > PlayerControlProgressBarFull.ActualWidth) newWidth = PlayerControlProgressBarFull.ActualWidth;

            PlayerControlProgressBarCompleted.Width = newWidth;

            double percentage = newWidth / PlayerControlProgressBarFull.ActualWidth;

            TimeSpan newTime = TimeSpan.FromTicks((long)(percentage * LibraryViewModel.Current.PlayQueue.TotalTicks));

            PlayerControlText.Text = newTime.ToString(@"%m\:ss") + @"/" + LibraryViewModel.Current.PlayQueue.TotalTime;

        }

        private void HandlePlayerControlProgressBarBezzelManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            double newWidth = e.Position.X;

            if (newWidth < 0) newWidth = 0;
            if (newWidth > PlayerControlProgressBarFull.ActualWidth) newWidth = PlayerControlProgressBarFull.ActualWidth;

            PlayerControlProgressBarCompleted.Width = newWidth;

            double percentage = newWidth / PlayerControlProgressBarFull.ActualWidth;

            LibraryViewModel.Current.PlayQueue.ScrubToPercentage(percentage);

            ProgressBarScrubView.Visibility = Visibility.Collapsed;
            
            PlayerControlProgressBarCompleted.SetBinding(Rectangle.WidthProperty, savedWidthBinding);
            savedWidthBinding = null;

            PlayerControlText.Text = string.Empty;
        }

        private void CurrentTrackPanel_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (Math.Abs(e.Cumulative.Translation.Y) < 10)
            {
                if (e.Cumulative.Translation.X > 30 && LibraryViewModel.Current.PlayQueue.PrevPlayer.CanExecute(null))
                {
                    LibraryViewModel.Current.PlayQueue.PrevPlayer.Execute(null);
                }
                else if (e.Cumulative.Translation.X < -30 && LibraryViewModel.Current.PlayQueue.SkipPlayer.CanExecute(null))
                {
                    LibraryViewModel.Current.PlayQueue.SkipPlayer.Execute(null);
                }
            }
        }
    }
}

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MusicMink.Controls
{
    public class ShiftedGrid : Grid
    {
        public double Center
        {
            get { return (double)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register(
            "Center", typeof(double),
            typeof(ShiftedGrid), new PropertyMetadata(0.0, HandleShiftedTextBlockPropertyChanged)
            );

        private static void HandleShiftedTextBlockPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShiftedGrid shiftedTextBlock = (ShiftedGrid)d;

            shiftedTextBlock.UpdateMargins();
        }

        public ShiftedGrid() : base()
        {
            this.Loaded += ShiftedGrid_Loaded;
            this.DataContextChanged += ShiftedGrid_DataContextChanged;
            this.SizeChanged += ShiftedGrid_SizeChanged;
        }

        void ShiftedGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateMargins();
        }

        void ShiftedGrid_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateMargins();
        }

        private void ShiftedGrid_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.UpdateMargins();
        }

        public void UpdateMargins()
        {
            double fullWidth = Windows.UI.Xaml.Window.Current.Bounds.Width - this.Margin.Left - this.Margin.Right;

            double newCenterPoint = this.Center - (this.ActualWidth / 2);

            if (newCenterPoint < 0) newCenterPoint = 0;
            if (newCenterPoint > fullWidth - (this.ActualWidth)) newCenterPoint = fullWidth - (this.ActualWidth);

            TranslateTransform tt = new TranslateTransform();
            tt.X = newCenterPoint;

            this.RenderTransform = tt;
        }
    }
}

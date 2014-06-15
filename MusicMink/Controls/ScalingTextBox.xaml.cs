using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MusicMink.Controls
{
    /// <summary>
    /// Scales the text to fit within the fixed width
    /// </summary>
    public partial class ScalingTextBox : UserControl
    {
        public static readonly DependencyProperty TextForegroundProperty =
            DependencyProperty.Register(
            "TextForeground", typeof(Brush),
            typeof(ScalingTextBox), new PropertyMetadata(null, HandleForegroundPropertyChanged)
            );

        public Brush TextForeground
        {
            get { return (Brush)GetValue(TextForegroundProperty); }
            set { SetValue(TextForegroundProperty, value); }
        }

        private static void HandleForegroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScalingTextBox scalingTextBox = (ScalingTextBox)d;

            if (scalingTextBox.TextForeground != null)
            {
                scalingTextBox.ScalingTextBoxContent.Foreground = scalingTextBox.TextForeground;
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
            "Text", typeof(string),
            typeof(ScalingTextBox), new PropertyMetadata(string.Empty, HandleScalingTextBoxTextPropertyChanged)
            );

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void HandleScalingTextBoxTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScalingTextBox scalingTextBox = (ScalingTextBox)d;

            scalingTextBox.ScalingTextBoxContent.Text = scalingTextBox.Text;

            scalingTextBox.ScalingTextBoxContent.FontSize = scalingTextBox.MaxFontSize;
        }

        public static readonly DependencyProperty MaxFontSizeProperty =
            DependencyProperty.Register(
            "MaxFontSize", typeof(double),
            typeof(ScalingTextBox), new PropertyMetadata(0.0, HandleScalingTextBoxPropertyChanged)
            );

        public double MaxFontSize
        {
            get { return (double)GetValue(MaxFontSizeProperty); }
            set { SetValue(MaxFontSizeProperty, value); }
        }

        public static readonly DependencyProperty MinFontSizeProperty =
            DependencyProperty.Register(
            "MinFontSize", typeof(double),
            typeof(ScalingTextBox), new PropertyMetadata(0.0, HandleScalingTextBoxPropertyChanged)
            );

        public double MinFontSize
        {
            get { return (double)GetValue(MinFontSizeProperty); }
            set { SetValue(MinFontSizeProperty, value); }
        }

        private bool isLoaded = false;

        private static void HandleScalingTextBoxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScalingTextBox scalingTextBox = (ScalingTextBox)d;

            scalingTextBox.ScalingTextBoxContent.FontSize = scalingTextBox.MaxFontSize;

            scalingTextBox.TriggerUpdate();
        }

        public ScalingTextBox()
        {
            InitializeComponent();
        }

        private void ScalingTextBoxContainer_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;

            ScalingTextBoxContent.Margin = new Thickness(0, 0, 0, 0);

            ScalingTextBoxContainer.LayoutUpdated += ScalingTextBoxContainer_LayoutUpdated;
        }

        void ScalingTextBoxContainer_LayoutUpdated(object sender, object e)
        {
            TriggerUpdate();
        }

        double cachedWidth = 0;
        private void TriggerUpdate()
        {
            if (cachedWidth != ScalingTextBoxContent.ActualWidth)
            {
                cachedWidth = ScalingTextBoxContent.ActualWidth;

                if (!isLoaded || ScalingTextBoxContainer.ActualWidth == 0 || ScalingTextBoxContainer.Visibility == Visibility.Collapsed) return;

                if (ScalingTextBoxContent.ActualWidth - ScalingTextBoxContainer.ActualWidth > 1)
                {
                    double keepHeight = ScalingTextBoxContainer.ActualHeight;

                    double ratio = ScalingTextBoxContainer.ActualWidth / ScalingTextBoxContent.ActualWidth;

                    double newFontSize = ScalingTextBoxContent.FontSize * ratio;

                    if (newFontSize < MinFontSize) newFontSize = MinFontSize;

                    ScalingTextBoxContent.FontSize = newFontSize;

                    ScalingTextBoxContainer.Height = keepHeight;
                }
            }
        }

    }
}

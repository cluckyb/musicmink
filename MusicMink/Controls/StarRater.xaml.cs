using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MusicMink.Controls
{
    /// <summary>
    /// Lets you set a rating between 1 and 5 stars, including half stars
    /// </summary>
    public sealed partial class StarRater : UserControl
    {
        private List<Path> starChunks = new List<Path>();

        public static readonly DependencyProperty ScaleToWidthProperty =
            DependencyProperty.Register(
            "ScaleToWidth", typeof(bool),
            typeof(StarRater), new PropertyMetadata(false)
            );

        public bool ScaleToWidth
        {
            get { return (bool)GetValue(ScaleToWidthProperty); }
            set { SetValue(ScaleToWidthProperty, value); }
        }

        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(
            "BorderColor", typeof(SolidColorBrush),
            typeof(StarRater), new PropertyMetadata(App.Current.Resources["PhoneForegroundBrush"], OnStarColorPropertyChanged)
            );

        public SolidColorBrush BorderColor
        {
            get { return (SolidColorBrush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public static readonly DependencyProperty StarColorProperty =
            DependencyProperty.Register(
            "StarColor", typeof(SolidColorBrush),
            typeof(StarRater), new PropertyMetadata(App.Current.Resources["PhoneAccentBrush"], OnStarColorPropertyChanged)
            );

        private static void OnStarColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StarRater starRater = (StarRater)d;

            starRater.UpdateStars();
        }

        public SolidColorBrush StarColor
        {
            get { return (SolidColorBrush)GetValue(StarColorProperty); }
            set { SetValue(StarColorProperty, value); }
        }

        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register(
            "Rating", typeof(uint),
            typeof(StarRater), new PropertyMetadata(0, OnRatingPropertyChanged)
            );

        private static void OnRatingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StarRater starRater = (StarRater)d;

            starRater.UpdateStars();
        }

        public uint Rating
        {
            get 
            {
                var v = GetValue(RatingProperty);

                if (v.GetType() == typeof(int))
                {
                    return (uint)(int)v;
                }

                return (uint)v;
            }
            set { SetValue(RatingProperty, value); }
        }

        public StarRater()
        {
            InitializeComponent();

            this.Loaded += StarRater_Loaded;
            this.DataContextChanged += StarRater_DataContextChanged;
        }

        void StarRater_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.UpdateStars();
        }

        void StarRater_Loaded(object sender, RoutedEventArgs e)
        {
            base.OnApplyTemplate();

            double s = 1;
            double d = this.ActualWidth;

            if (d > 0 && this.ScaleToWidth)
            {
                s = d / 300;
            }

            RootStackPanel.Children.Clear();
            starChunks.Clear();

            for (uint i = 1; i <= 10; i += 2)
            {
                Grid newGrid = new Grid();

                Path leftHalf = CreatePath(s, i, false);
                Path rightHalf = CreatePath(s, i + 1, true);

                newGrid.Children.Add(leftHalf);
                newGrid.Children.Add(rightHalf);

                starChunks.Add(leftHalf);
                starChunks.Add(rightHalf);

                RootStackPanel.Children.Add(newGrid);
            }

            UpdateStars();
        }

        private void UpdateStars()
        {
            if (starChunks.Count == 0) return;

            uint curRating = Rating;

            for (int i = 0; i < starChunks.Count; i++)
            {
                starChunks[i].Stroke = BorderColor;
                if (i < curRating)
                {
                    starChunks[i].Fill = StarColor;
                }
                else
                {
                    starChunks[i].Fill = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        private void Region1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Rating = 1;
        }

        private void Region2_Tap(object sender, TappedRoutedEventArgs e)
        {
            Rating = 2;
        }

        private void Region3_Tap(object sender, TappedRoutedEventArgs e)
        {
            Rating = 3;
        }

        private void Region4_Tap(object sender, TappedRoutedEventArgs e)
        {
            Rating = 4;
        }

        private void Region5_Tap(object sender, TappedRoutedEventArgs e)
        {
            Rating = 5;
        }

        private void Region6_Tap(object sender, TappedRoutedEventArgs e)
        {
            Rating = 6;
        }

        private void Region7_Tap(object sender, TappedRoutedEventArgs e)
        {
            Rating = 7;
        }

        private void Region8_Tap(object sender, TappedRoutedEventArgs e)
        {
            Rating = 8;
        }

        private void Region9_Tap(object sender, TappedRoutedEventArgs e)
        {
            Rating = 9;
        }

        private void Region10_Tap(object sender, TappedRoutedEventArgs e)
        {
            Rating = 10;
        }


        private Path CreatePath(double scale, uint rating, bool isRightHalf)
        {
            Path newPath = new Path();

            newPath.Fill = new SolidColorBrush(Colors.Transparent);
            newPath.StrokeThickness = 3;
            newPath.Stroke = BorderColor;
            newPath.Tapped += (object sender, TappedRoutedEventArgs e) =>
                {
                    Rating = rating;
                };

            double adj = isRightHalf ? -60 : 0;
            double adjScale = isRightHalf ? -scale : scale;

            PathFigure pathData = new PathFigure();
            pathData.StartPoint = new Windows.Foundation.Point((adj + 30.5) * adjScale, 0 * scale);
            pathData.IsClosed = false;
            pathData.IsFilled = true;

            pathData.Segments.Add(new LineSegment() { Point = new Windows.Foundation.Point((adj + 20.2917) * adjScale, 12.1572 * scale) });
            pathData.Segments.Add(new LineSegment() { Point = new Windows.Foundation.Point((adj + 5.7296) * adjScale, 17.63355 * scale) });
            pathData.Segments.Add(new LineSegment() { Point = new Windows.Foundation.Point((adj + 14.2917) * adjScale, 30.6234 * scale) });
            pathData.Segments.Add(new LineSegment() { Point = new Windows.Foundation.Point((adj + 15) * adjScale, 46.1652 * scale) });
            pathData.Segments.Add(new LineSegment() { Point = new Windows.Foundation.Point((adj + 30.5) * adjScale, 42.0361 * scale) });


            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathData);

            newPath.Data = pathGeometry;

            return newPath;

            /*
                                x:Name="Region1"
                                Fill="Transparent"
                                Stroke="{StaticResource PhoneForegroundBrush}"
                                StrokeThickness="3"
                                Tapped="Region1_Tapped"
             * 
             * <PathFigure StartPoint="30.5,0" IsClosed="False">
                                                <LineSegment Point="20.2917,12.1572" />
                                                <LineSegment Point="5.7296,17.63355" />
                                                <LineSegment Point="14.2917,30.6234" />
                                                <LineSegment Point="15,46.1652" />
                                                <LineSegment Point="30.5,42.0361" />
                                            </PathFigure>
             * 
                                            <PathFigure StartPoint="29.5,0" IsClosed="False">
                                                <LineSegment Point="39.7082,12.1572" />
                                                <LineSegment Point="54.2705,17.63355" />
                                                <LineSegment Point="45.7082,30.6234" />
                                                <LineSegment Point="45,46.1652" />
                                                <LineSegment Point="29.5,42.0361" />
                                            </PathFigure>
                                        </PathGeometry>
 
             * 
             * 
            */
        }

        void newPath_Tapped(object sender, TappedRoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}

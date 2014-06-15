using System.Collections.Generic;
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
        }

        void StarRater_Loaded(object sender, RoutedEventArgs e)
        {
            base.OnApplyTemplate();

            starChunks.Add(Region1);
            starChunks.Add(Region2);
            starChunks.Add(Region3);
            starChunks.Add(Region4);
            starChunks.Add(Region5);
            starChunks.Add(Region6);
            starChunks.Add(Region7);
            starChunks.Add(Region8);
            starChunks.Add(Region9);
            starChunks.Add(Region10);

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
    }
}

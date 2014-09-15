using MusicMinkAppLayer.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MusicMink.Controls
{
    public sealed partial class SymbolTile : Button
    {
        public static readonly DependencyProperty IsProgressProperty =
            DependencyProperty.Register(
            "IsProgress", typeof(bool),
            typeof(SymbolTile), new PropertyMetadata(false, OnIsProgressPropertyChanged)
            );

        private static void OnIsProgressPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SymbolTile symbolTile = (SymbolTile)d;

            symbolTile.UpdateProgress();
        }

        public bool IsProgress
        {
            get { return (bool)GetValue(IsProgressProperty); }
            set { SetValue(IsProgressProperty, value); }
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register(
            "Caption", typeof(string),
            typeof(SymbolTile), new PropertyMetadata(string.Empty, OnCaptionPropertyChanged)
            );

        private static void OnCaptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SymbolTile symbolTile = (SymbolTile)d;

            symbolTile.UpdateCaption();
        }

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(
            "Symbol", typeof(Symbol),
            typeof(SymbolTile), new PropertyMetadata(Symbol.Placeholder, OnSymbolPropertyChanged)
            );

        private static void OnSymbolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SymbolTile symbolTile = (SymbolTile)d;

            symbolTile.UpdateSymbol();
        }

        public Symbol Symbol
        {
            get { return (Symbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty InnerMarginProperty =
            DependencyProperty.Register(
            "InnerMargin", typeof(double),
            typeof(SymbolTile), new PropertyMetadata(15.0, OnInnerMarginPropertyChanged)
            );

        private static void OnInnerMarginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SymbolTile symbolTile = (SymbolTile)d;

            symbolTile.UpdateMargins();
        }

        public double InnerMargin
        {
            get { return (double)GetValue(InnerMarginProperty); }
            set { SetValue(InnerMarginProperty, value); }
        }
        
        public SymbolTile()
        {
            this.InitializeComponent();

            this.Loaded += SymbolTile_Loaded;

            this.SizeChanged += SymbolTile_SizeChanged;
        }

        void SymbolTile_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMargins();
        }


        void SymbolTile_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMargins();
            UpdateProgress();
        }

        private void UpdateCaption()
        {
            CaptionTextBlock.Text = this.Caption;
        }

        private void UpdateSymbol()
        {
            SymbolIcon.Symbol = this.Symbol;
        }

        private void UpdateProgress()
        {
            if (this.IsProgress)
            {
                ProgressIcon.Visibility = Visibility.Visible;
                SymbolIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProgressIcon.Visibility = Visibility.Collapsed;
                SymbolIcon.Visibility = Visibility.Visible;
            }
        }

        private void UpdateMargins()
        {
            double h = this.ActualHeight == 0 ? this.Height : this.ActualHeight;
            double w = this.ActualWidth == 0 ? this.Width : this.ActualWidth;

            double desiredHeight = h - 2 * InnerMargin;
            double desiredWidth = w - 2 * InnerMargin;

            SymbolTileScaleTransform.ScaleX = desiredHeight / 30.0;
            SymbolTileScaleTransform.ScaleY = desiredWidth / 30.0;

            // For some reason the visual states don't work... so giving up and doing this instead
            if (w < 160)
            {
                CaptionTextBlock.Style = DebugHelper.CastAndAssert<Style>(Resources["TileTextStyleSmall"]);
            }
            else
            {
                CaptionTextBlock.Style = DebugHelper.CastAndAssert<Style>(Resources["TileTextStyle"]);
            }
        }

    }
}

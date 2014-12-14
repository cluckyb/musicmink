using System;
using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MusicMink.Converters
{
    public class ColorToStyleConverter : IValueConverter
    {
        public Style TargetStyle { get; set; }

        public SolidColorBrush TargetBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush valueAsBrush = DebugHelper.CastAndAssert<SolidColorBrush>(value);

            if (valueAsBrush == TargetBrush)
            {
                return TargetStyle;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

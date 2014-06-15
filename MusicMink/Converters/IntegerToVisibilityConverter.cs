using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MusicMink.Converters
{
    class IntegerToVisibilityConverter : IValueConverter
    {
        public int PivotPoint { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && ((int)value > PivotPoint))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

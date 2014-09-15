using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MusicMink.Converters
{
    class EnumToVisibilityConverter : IValueConverter
    {
        public int Target { get; set; }

        public bool Invert { get; set; } 

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (Invert ^ (value != null && ((int)value == Target)))
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

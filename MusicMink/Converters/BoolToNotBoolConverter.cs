using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MusicMink.Converters
{
    class BoolToNotBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

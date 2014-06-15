using System;
using Windows.UI.Xaml.Data;

namespace MusicMink.Converters
{
    class BoolToObjectConverter : IValueConverter
    {
        public bool ValueIfTrue { get; set; }
        public bool ValueIfFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((value != null && (bool)value))
            {
                return ValueIfTrue;
            }

            return ValueIfFalse;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MusicMink.Converters
{
    class NullToVisibilityConverter : IValueConverter
    {
        private bool _invert = false;
        public bool Invert
        {
            get
            {
                return _invert;
            }
            set
            {
                _invert = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((value == null) ^ Invert)
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

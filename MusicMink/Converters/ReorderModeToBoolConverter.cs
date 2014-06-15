using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MusicMink.Converters
{
    class ReorderModeToBoolConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((ListViewReorderMode)value == ListViewReorderMode.Enabled) return (true ^ Invert);

            return (false ^ Invert);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((bool)(value) ^ Invert) return ListViewReorderMode.Enabled;

            return ListViewReorderMode.Disabled;
        }
    }
}

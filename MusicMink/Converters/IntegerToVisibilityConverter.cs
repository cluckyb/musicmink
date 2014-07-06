using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MusicMink.Converters
{
    public enum ComparisionMode
    {
        Greater,
        Equal
    }

    class IntegerToVisibilityConverter : IValueConverter
    {
        public int PivotPoint { get; set; }

        public ComparisionMode CompareMode { get; set; } 

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (CompareMode == ComparisionMode.Greater)
            {
                if (value != null && ((int)value > PivotPoint))
                {
                    return Visibility.Visible;
                }
            }
            else if (CompareMode == ComparisionMode.Equal)
            {
                if (value != null && ((int)value == PivotPoint))
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

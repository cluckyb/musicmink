using MusicMinkAppLayer.Diagnostics;
using System;
using Windows.UI.Xaml.Data;

namespace MusicMink.Converters
{
    class PercentageTransformConverter : IValueConverter
    {
        public int FullTarget { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double valueAsDouble = DebugHelper.CastAndAssert<double>(value);

            if (valueAsDouble < 0) return 0;
            if (valueAsDouble > FullTarget) return FullTarget;

            return FullTarget * valueAsDouble;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

using MusicMinkAppLayer.Diagnostics;
using System;
using Windows.UI.Xaml.Data;

namespace MusicMink.Converters
{
    class PercentageTransformConverter : IValueConverter
    {
        public int FullTarget { get; set; }

        private bool _fullMode = false;
        public bool FullMode
        {
            get
            {
                return _fullMode;
            }
            set
            {
                _fullMode = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double valueAsDouble = DebugHelper.CastAndAssert<double>(value);

            double target = FullTarget;
            if (FullMode) target = Windows.UI.Xaml.Window.Current.Bounds.Width;

            if (valueAsDouble < 0) return 0;
            if (valueAsDouble > target) return target;

            return target * valueAsDouble;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

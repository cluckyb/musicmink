using MusicMinkAppLayer.Diagnostics;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MusicMink.Converters
{
    class ProgressTriangleConverter : IValueConverter
    {
        private Polygon _baseShape = null;
        public Polygon BaseShape
        {
            get
            {
                return _baseShape;
            }
            set
            {
                _baseShape = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double valueAsDouble = DebugHelper.CastAndAssert<double>(value);

            double LeftBase = 20;

            double RightBase = 20;

            if (BaseShape == null)
            {
                BaseShape = new Polygon();

                BaseShape.Points.Add(new Point(-20,25));
                BaseShape.Points.Add(new Point(-20,20));
                BaseShape.Points.Add(new Point(0,0));
                BaseShape.Points.Add(new Point(20,20));
                BaseShape.Points.Add(new Point(20,25));
            }

            PointCollection newShape = new PointCollection();

            double maxValue = Windows.UI.Xaml.Window.Current.Bounds.Width;

            if (BaseShape != null)
            {
                foreach (Point j in BaseShape.Points)
                {
                    double shiftX = j.X;

                    if (valueAsDouble < LeftBase)
                    {
                        if (j.X < 0)
                        {
                            shiftX = j.X * valueAsDouble / LeftBase;
                        }
                    }
                    if ((maxValue - valueAsDouble) < RightBase)
                    {
                        if (j.X > 0)
                        {
                            shiftX = j.X * (maxValue - valueAsDouble) / RightBase;
                        }
                    }

                    newShape.Add(new Point(shiftX, j.Y));
                }
            }

            return newShape;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

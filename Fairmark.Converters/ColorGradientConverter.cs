using System;
using Windows.UI.Xaml.Media;

namespace Fairmark.Converters
{
    public class ColorGradientConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new SolidColorBrush(Windows.UI.Colors.Transparent);
            if (value is Windows.UI.Color color)
            {
                LinearGradientBrush brush = new LinearGradientBrush
                {
                    Opacity = 0.2,
                    StartPoint = new Windows.Foundation.Point(0, 0),
                    EndPoint = new Windows.Foundation.Point(1, 0)
                };
                brush.GradientStops.Add(new GradientStop
                {
                    Color = color,
                    Offset = 0
                });
                brush.GradientStops.Add(new GradientStop
                {
                    Color = Windows.UI.Colors.Transparent,
                    Offset = 1
                });
                return brush;
            }
            else if (value is Brush brush)
            {
                return brush;
            }
            else
            {
                throw new ArgumentException("Value must be a Color or SolidColorBrush.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
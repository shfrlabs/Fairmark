using System;

namespace Fairmark.Converters
{
    public class ColorBrushConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Windows.UI.Color color)
            {
                return new Windows.UI.Xaml.Media.SolidColorBrush(color);
            }
            else if (value is Windows.UI.Xaml.Media.SolidColorBrush brush)
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
            if (value is Windows.UI.Xaml.Media.SolidColorBrush brush)
            {
                return brush.Color;
            }
            else if (value is Windows.UI.Color color)
            {
                return color;
            }
            else
            {
                throw new ArgumentException("Value must be a SolidColorBrush or Color.");
            }
        }
    }
}
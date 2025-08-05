using System;
using Windows.UI.Xaml.Data;

namespace Fairmark.Converters
{
    public class FontFamilyToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Windows.UI.Xaml.Media.FontFamily fontFamily)
            {
                return fontFamily.Source;
            }
            if (value is string fontName && !string.IsNullOrEmpty(fontName))
            {
                return new Windows.UI.Xaml.Media.FontFamily(fontName);
            }
            else if (value == null)
            {
                return new Windows.UI.Xaml.Media.FontFamily("Consolas");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Windows.UI.Xaml.Media.FontFamily fontFamily)
            {
                return fontFamily.Source;
            }
            if (value is string fontName && !string.IsNullOrEmpty(fontName))
            {
                return new Windows.UI.Xaml.Media.FontFamily(fontName);
            }
            else if (value == null)
            {
                return new Windows.UI.Xaml.Media.FontFamily("Consolas");
            }
            return string.Empty;
        }
    }
}

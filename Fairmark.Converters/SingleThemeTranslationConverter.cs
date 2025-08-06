using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Fairmark.Converters
{
    public class SingleThemeTranslationConverter : IValueConverter
    {
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string theme)
            {
                return resourceLoader.GetString($"{theme}ThemeName") ?? theme;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string displayName)
            {
                if (displayName == resourceLoader.GetString("LightThemeName"))
                    return "Light";
                if (displayName == resourceLoader.GetString("DarkThemeName"))
                    return "Dark";
                if (displayName == resourceLoader.GetString("SystemThemeName"))
                    return "System";
            }
            return value;
        }
    }
}
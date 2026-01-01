using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Fairmark.Converters {
    public class SingleThemeTranslationConverter : IValueConverter {
        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView();
        private readonly string[] _originalThemes = { "Default", "Light", "Dark" };

        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value is string theme) {
                var resourceKey = $"Theme_{theme}";
                var localizedValue = _resourceLoader.GetString(resourceKey);
                return string.IsNullOrEmpty(localizedValue) ? theme : localizedValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            if (value is string localizedTheme) {
                foreach (var originalTheme in _originalThemes) {
                    var resourceKey = $"Theme_{originalTheme}";
                    var localizedValue = _resourceLoader.GetString(resourceKey);
                    var compareValue = string.IsNullOrEmpty(localizedValue) ? originalTheme : localizedValue;

                    if (string.Equals(compareValue, localizedTheme, StringComparison.OrdinalIgnoreCase)) {
                        return originalTheme;
                    }
                }

                return localizedTheme;
            }
            return value;
        }
    }
}
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
                // Find the original theme name by checking which localized string matches
                foreach (var originalTheme in _originalThemes) {
                    var resourceKey = $"Theme_{originalTheme}";
                    var localizedValue = _resourceLoader.GetString(resourceKey);
                    var compareValue = string.IsNullOrEmpty(localizedValue) ? originalTheme : localizedValue;

                    if (string.Equals(compareValue, localizedTheme, StringComparison.OrdinalIgnoreCase)) {
                        return originalTheme;
                    }
                }

                // Fallback: return the value as-is if no match found
                return localizedTheme;
            }
            return value;
        }
    }
}
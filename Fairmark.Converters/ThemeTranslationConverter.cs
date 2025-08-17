using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Fairmark.Converters {
    public class ThemeTranslationConverter : IValueConverter {
        private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView();

        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value is string[] themes) {
                var localizedThemes = new string[themes.Length];
                for (int i = 0; i < themes.Length; i++) {
                    var resourceKey = $"Theme_{themes[i]}";
                    var localizedValue = _resourceLoader.GetString(resourceKey);
                    localizedThemes[i] = string.IsNullOrEmpty(localizedValue) ? themes[i] : localizedValue;
                }
                return localizedThemes;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException("ThemeTranslationConverter is one-way only");
        }
    }
}
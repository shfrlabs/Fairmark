using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Fairmark.Converters
{
    public class ThemeTranslationConverter : IValueConverter
    {
        internal ResourceLoader _resourceLoader = ResourceLoader.GetForCurrentView();
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            List<string> themes = new List<string>();
            if (value is string[] themelist)
            {
                foreach (string theme in themelist)
                {
                    themes.Add(_resourceLoader.GetString($"{theme}ThemeName"));
                }
            }
            return themes.ToArray();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

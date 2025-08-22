using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Fairmark.Converters {
    public class GroupHeaderNameConverter : IValueConverter {
        private ResourceLoader loader = ResourceLoader.GetForCurrentView();
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value is NoteGroup key) {
                if (key.Count > 0) {
                    return loader.GetString(key.Key);
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}

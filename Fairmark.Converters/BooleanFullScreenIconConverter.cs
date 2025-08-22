using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Fairmark.Converters {
    public class BooleanFullScreenIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value is bool boolValue) {
                Debug.WriteLine($"BooleanFullScreenIconConverter: {boolValue}");
                return boolValue ? "\uE73F" : "\uE740";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}

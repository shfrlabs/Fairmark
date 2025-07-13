using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Fairmark.Converters {
    public class EditorSizeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            try {
                if (value is float floatVal && targetType == typeof(double))
                    return (double)floatVal;

                if (value is string stringVal && double.TryParse(stringVal, out double result))
                    return result;

                return value;
            }
            catch {
                return 0d;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            try {
                if (value is double doubleVal && targetType == typeof(float))
                    return (float)doubleVal;

                // Handle string input (just in case)
                if (value is string stringVal && float.TryParse(stringVal, out float result))
                    return result;

                return value;
            }
            catch {
                return 0f;
            }
        }
    }
}

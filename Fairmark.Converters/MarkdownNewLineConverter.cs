using System;
using System.Globalization;
using System.Text;
using Windows.UI.Xaml.Data;

namespace Fairmark.Converters {
    public class MarkdownNewLineConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var input = value as string;
            if (string.IsNullOrEmpty(input))
                return value;

            var lines = input.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            for (int i = 0; i < lines.Length; i++) {
                var line = lines[i].TrimEnd();

                if (string.IsNullOrWhiteSpace(line)) {
                    sb.Append("\n\n");
                }
                else {
                    sb.Append(line + "  \n");
                }
            }

            var result = sb.ToString().TrimEnd(); // Remove trailing newlines
            System.Diagnostics.Debug.WriteLine($"[MarkdownNewLineConverter] Converted:\n{result}");
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return (value as string)?.Replace("  \n", "\n").Replace("\n\n", "\n");
        }
    }
}
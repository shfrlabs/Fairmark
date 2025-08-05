using Fairmark.Models;
using System;
using System.Collections.ObjectModel;

namespace Fairmark.Converters
{
    public class CountVisibilityConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((ObservableCollection<NoteMetadata>)value != null)
            {
                if (((ObservableCollection<NoteMetadata>)value).Count > 0)
                {
                    return Windows.UI.Xaml.Visibility.Collapsed;
                }
                else
                {
                    return Windows.UI.Xaml.Visibility.Visible;
                }
            }
            return Windows.UI.Xaml.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("ConvertBack is not implemented for CountVisibilityConverter.");
        }
    }
}
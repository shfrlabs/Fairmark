using Fairmark.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace Fairmark.Converters
{
    public class TagGradientConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null || !(value is IEnumerable<NoteTag> tags))
            {
                return new SolidColorBrush(Windows.UI.Colors.Transparent);
            }
            else
            {
                LinearGradientBrush brush = new LinearGradientBrush()
                {
                    Opacity = 0.07,
                    StartPoint = new Windows.Foundation.Point(0, 0),
                    EndPoint = new Windows.Foundation.Point(1, 0)
                };
                var orderedTags = ((ObservableCollection<NoteTag>)value).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
                for (int i = 0; i < orderedTags.Count; i++)
                {
                    var tag = orderedTags[i];
                    double offset = (orderedTags.Count == 1) ? 0.5 : (double)i / (orderedTags.Count - 1);
                    brush.GradientStops.Add(new GradientStop()
                    {
                        Color = tag.Color,
                        Offset = offset
                    });
                }
                return brush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("ConvertBack is not implemented for TagGradientConverter.");
        }
    }
}
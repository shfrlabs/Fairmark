using Fairmark.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    Opacity = 0.2,
                    StartPoint = new Windows.Foundation.Point(0, 0),
                    EndPoint = new Windows.Foundation.Point(((ObservableCollection<NoteTag>)value).Count, 0)
                };
                foreach (NoteTag tag in (ObservableCollection<NoteTag>)value)
                {
                    brush.GradientStops.Add(new GradientStop()
                    {
                        Color = tag.Color,
                        Offset = brush.GradientStops.Count/1.6
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
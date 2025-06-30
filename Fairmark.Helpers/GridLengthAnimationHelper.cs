using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fairmark.Helpers
{
    public class GridLengthAnimationHelper : DependencyObject
    {
        public double AnimatedValue
        {
            get { return (double)GetValue(AnimatedValueProperty); }
            set { SetValue(AnimatedValueProperty, value); }
        }

        public static readonly DependencyProperty AnimatedValueProperty =
            DependencyProperty.Register("AnimatedValue", typeof(double), typeof(GridLengthAnimationHelper),
                new PropertyMetadata(0.0, OnAnimatedValueChanged));

        private static void OnAnimatedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridLengthAnimationHelper helper && helper.TargetColumn != null)
            {
                helper.TargetColumn.Width = new GridLength((double)e.NewValue);
            }
        }

        public ColumnDefinition TargetColumn { get; set; }
    }
}
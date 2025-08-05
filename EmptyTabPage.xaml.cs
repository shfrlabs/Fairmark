using Fairmark.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fairmark
{
    public sealed partial class EmptyTabPage : Page
    {
        public EmptyTabPage()
        {
            this.InitializeComponent();
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, e) =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    frame.RequestedTheme = e.Theme;
                }
            };
        }
    }
}

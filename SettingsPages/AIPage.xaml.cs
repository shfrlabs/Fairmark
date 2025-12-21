using Fairmark.Helpers;
using Fairmark.Intelligence;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Fairmark.SettingsPages
{
    public sealed partial class AIPage : Page
    {
        public AIPage()
        {
            this.InitializeComponent();
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, e) =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    frame.RequestedTheme = e.Theme;
                }
            };
            ais?.RefreshModels();
        }

        public AISettings ais => App.AISettings;
    }
}

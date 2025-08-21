using Fairmark.Helpers;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fairmark
{
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, e) =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    frame.RequestedTheme = e.Theme;
                }
                var view = ApplicationView.GetForCurrentView();
                if (e.Theme == ElementTheme.Dark)
                {
                    view.TitleBar.ForegroundColor = Colors.White;
                    view.TitleBar.ButtonForegroundColor = Colors.White;
                }
                else
                {
                    view.TitleBar.ForegroundColor = Colors.Black;
                    view.TitleBar.ButtonForegroundColor = Colors.Black;
                }
            };
        }

        public string BetaNumber {
            get {
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                return "Thank you for participating in beta " + version.Minor.ToString() + "!";
            }
        }

        public string Version
        {
            get
            {
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }
    }
}

using Fairmark.Helpers;
using System;
using Windows.Services.Store;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fairmark
{
    public sealed partial class ContactPage : Page
    {
        private StoreContext _storeContext;

        public ContactPage()
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

        private async void RateButton_Click(object sender, RoutedEventArgs e)
        {
            _storeContext = StoreContext.GetDefault();
            _ = await _storeContext.RequestRateAndReviewAppAsync();
        }
    }
}

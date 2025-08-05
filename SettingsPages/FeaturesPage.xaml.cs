using Fairmark.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fairmark.SettingsPages
{
    public sealed partial class FeaturesPage : Page
    {
        public FeaturesPage()
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

        private async void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            if (await Windows.Security.Credentials.UI.UserConsentVerifier.CheckAvailabilityAsync() != Windows.Security.Credentials.UI.UserConsentVerifierAvailability.Available)
            {
                (sender as ToggleSwitch).IsOn = false;
                (sender as ToggleSwitch).IsEnabled = false;
            }
        }
    }
}

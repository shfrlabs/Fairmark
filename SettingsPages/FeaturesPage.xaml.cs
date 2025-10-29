using Fairmark.Helpers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Security.Credentials.UI;
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

        private async void ScreenshotToggle_Loaded(object sender, RoutedEventArgs e) {
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e) {
            var toggle = (ToggleSwitch)sender;
            toggle.Toggled -= ToggleSwitch_Toggled;
            var settings = (Application.Current.Resources["Settings"] as Settings);
            var loader = ResourceLoader.GetForCurrentView();

            if (!toggle.IsOn)
            {
                toggle.IsOn = true;

                var availability = await UserConsentVerifier.CheckAvailabilityAsync();
                if (availability != UserConsentVerifierAvailability.Available) {
                    await new ContentDialog {
                        Title = loader.GetString("AuthOffDialogTitle"),
                        Content = loader.GetString("AuthOffDialogDesc"),
                        PrimaryButtonText = loader.GetString("OK")
                    }.ShowAsync();
                    return;
                }

                var result = await UserConsentVerifier.RequestVerificationAsync(loader.GetString("AuthTitle"));
                if (result == UserConsentVerificationResult.Verified) {
                    settings.AuthenticationEnabled = false;
                    toggle.IsOn = false;
                }
            }
            toggle.Toggled += ToggleSwitch_Toggled;

        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fairmark.SettingsPages {
    public sealed partial class FeaturesPage : Page {
        public FeaturesPage() {
            this.InitializeComponent();
        }

        private async void ToggleSwitch_Loaded(object sender, RoutedEventArgs e) {
            if (await Windows.Security.Credentials.UI.UserConsentVerifier.CheckAvailabilityAsync() != Windows.Security.Credentials.UI.UserConsentVerifierAvailability.Available) {
                (sender as ToggleSwitch).IsOn = false;
                (sender as ToggleSwitch).IsEnabled = false;
            }
        }
    }
}

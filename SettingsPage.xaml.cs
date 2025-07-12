using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Security.Credentials.UI;
using Windows.UI.Xaml.Navigation;
using Fairmark.Helpers;
using Fairmark.SettingsPages;


namespace Fairmark {
    public sealed partial class SettingsPage : Page {
        public SettingsPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (e.Parameter != null && e.Parameter.ToString() == "OOBE") {
                RootGrid.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
            }
            SettingsNav.SelectedItem = SettingsNav.MenuItems.First();
        }

        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args) {
            switch ((args.SelectedItem as Microsoft.UI.Xaml.Controls.NavigationViewItem).Tag as string) {
                case "AI":
                    settingsFrame.Navigate(typeof(AIPage)); break;
                case "Display":
                    settingsFrame.Navigate(typeof(DisplayPage)); break;
                case "Features":
                    settingsFrame.Navigate(typeof(FeaturesPage)); break;
                case "ImExport":
                    settingsFrame.Navigate(typeof(ImportExportPage)); break;
                case "Logs":
                    settingsFrame.Navigate(typeof(AccessLogsPage)); break;
                case "Stats":
                    settingsFrame.Navigate(typeof(StatsPage)); break;
                default:
                    settingsFrame.Content = null; break;
            }
        }

        private void settingsFrame_Loaded(object sender, RoutedEventArgs e) {
            settingsFrame.Navigate(typeof(AIPage));
        }
    }
}

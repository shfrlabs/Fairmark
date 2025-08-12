using Fairmark.Helpers;
using Fairmark.SettingsPages;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;


namespace Fairmark
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null && e.Parameter.ToString() == "OOBE")
            {
                RootGrid.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                SettingsNav.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top;
                settingsFrame.Height = 400;
            }
            SettingsNav.SelectedItem = SettingsNav.MenuItems.First();
        }

        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            switch ((args.SelectedItem as Microsoft.UI.Xaml.Controls.NavigationViewItem).Tag as string)
            {
                case "AI":
                    _ = settingsFrame.Navigate(typeof(AIPage)); break;
                case "Display":
                    _ = settingsFrame.Navigate(typeof(DisplayPage)); break;
                case "Features":
                    _ = settingsFrame.Navigate(typeof(FeaturesPage)); break;
                case "ImExport":
                    _ = settingsFrame.Navigate(typeof(ImportExportPage)); break;
                case "Logs":
                    _ = settingsFrame.Navigate(typeof(AccessLogsPage)); break;
                case "Stats":
                    _ = settingsFrame.Navigate(typeof(StatsPage)); break;
                case "Tags":
                    _ = settingsFrame.Navigate(typeof(TagManagerPage)); break;
                case "Upgrade":
                    _ = settingsFrame.Navigate(typeof(UpgradePage)); break;
                default:
                    settingsFrame.Content = null; break;
            }
        }

        private void settingsFrame_Loaded(object sender, RoutedEventArgs e)
        {
            _ = settingsFrame.Navigate(typeof(DisplayPage));
        }

        private async void NavigationViewItem_Loaded(object sender, RoutedEventArgs e) {
            (sender as NavigationViewItem).Visibility = await Variables.CheckIfPlusAsync() ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

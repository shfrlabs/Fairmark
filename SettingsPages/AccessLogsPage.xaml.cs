using Fairmark.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fairmark.SettingsPages
{

    public sealed partial class AccessLogsPage : Page
    {
        public AccessLogsPage()
        {
            this.InitializeComponent();
        }
        public string logText => App.LogHelper.logs;

        private void CopyLogs_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(logText);
            Clipboard.SetContent(dataPackage);
        }

        private void CopyDebugLogs_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(Anonymize(logText));
            Clipboard.SetContent(dataPackage);
        }

        private string Anonymize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return System.Text.RegularExpressions.Regex.Replace(
                text,
                @"'[^']*'",
                "'***'"
            );
        }

        private async void ToggleSwitch_Loaded(object sender, RoutedEventArgs e) {
        }
    }
}

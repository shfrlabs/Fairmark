using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
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

    public sealed partial class AccessLogsPage : Page {
        public AccessLogsPage() {
            this.InitializeComponent();
        }
        public string logText => App.LogHelper.logs;

        private void CopyLogs_Click(object sender, RoutedEventArgs e) {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(logText);
            Clipboard.SetContent(dataPackage);
        }

        private void CopyDebugLogs_Click(object sender, RoutedEventArgs e) {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(Anonymize(logText));
            Clipboard.SetContent(dataPackage);
        }

        private string Anonymize(string text) {
            if (string.IsNullOrEmpty(text))
                return text;
            return System.Text.RegularExpressions.Regex.Replace(
                text,
                @"'[^']*'",
                "'***'"
            );
        }
    }
}

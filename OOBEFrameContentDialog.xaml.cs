using Fairmark.OOBEPages;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Fairmark {
    public sealed partial class OOBEFrameContentDialog : ContentDialog {
        public OOBEFrameContentDialog() {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e) {
            contentFrame.Navigate(typeof(SettingsPage), "OOBE");
            ContinueButton.Visibility = Visibility.Collapsed;
            FinishButton.Visibility = Visibility.Visible;
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void contentFrame_Loaded(object sender, RoutedEventArgs e) {
            contentFrame.Navigate(typeof(Welcome));
            FinishButton.Visibility = Visibility.Collapsed;
        }
    }
}

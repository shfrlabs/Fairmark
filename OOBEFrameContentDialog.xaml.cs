using Fairmark.Helpers;
using Fairmark.OOBEPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fairmark
{
    public sealed partial class OOBEFrameContentDialog : ContentDialog
    {
        public OOBEFrameContentDialog()
        {
            this.InitializeComponent();
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, e) =>
            {
                this.RequestedTheme = e.Theme;
            };
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            _ = contentFrame.Navigate(typeof(SettingsPage), "OOBE");
            ContinueButton.Visibility = Visibility.Collapsed;
            FinishButton.Visibility = Visibility.Visible;
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void contentFrame_Loaded(object sender, RoutedEventArgs e)
        {
            _ = contentFrame.Navigate(typeof(Welcome));
            FinishButton.Visibility = Visibility.Collapsed;
        }
    }
}

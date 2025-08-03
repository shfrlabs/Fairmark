using Fairmark.Helpers;
//using Fairmark.Intelligence;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Fairmark.SettingsPages {
    public sealed partial class AIPage : Page {
        public AIPage() {
            this.InitializeComponent();
            // Add theme event handler for AIPage
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, e) =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    frame.RequestedTheme = e.Theme;
                }
            };
        }
        //public ModelHelper _modelHelper = new ModelHelper();

        private void DownloadModel_Loaded(object sender, RoutedEventArgs e) {
            //bool isDownloaded = false;
            //try {
            //    string model = (sender as Button).Tag.ToString();
            //    isDownloaded = _modelHelper.CheckModelState(model);
            //}
            //finally {
            //    if (isDownloaded) {
            //        (sender as Button).Content = "Remove";
            //    }
            //    else {
            //        (sender as Button).Content = "Download";
            //    }
            //}
        }

        private async void DownloadModel_Click(object sender, RoutedEventArgs e) {
            //bool isDownloaded = false;
            //string model = string.Empty;
            //(sender as Button).IsEnabled = false;
            //try {
            //    model = (sender as Button).Tag.ToString();
            //    isDownloaded = _modelHelper.CheckModelState(model);
            //}
            //finally {
            //    if (isDownloaded) {
            //        _modelHelper.DeleteModel(model);
            //    }
            //    else {
            //        await _modelHelper.DownloadModel(model);
            //    }
            //}
            //(sender as Button).IsEnabled = true;
            //DownloadModel_Loaded(sender, e);
        }
    }
}

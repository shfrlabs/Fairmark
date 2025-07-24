using Fairmark.Intelligence;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Fairmark.SettingsPages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AIPage : Page {
        public AIPage() {
            this.InitializeComponent();
        }
        public ModelHelper _modelHelper = new ModelHelper();

        private void DownloadModel_Loaded(object sender, RoutedEventArgs e) {
            bool isDownloaded = false;
            try {
                string model = (sender as Button).Tag.ToString();
                isDownloaded = _modelHelper.CheckModelState(model);
            }
            finally {
                if (isDownloaded) {
                    (sender as Button).Content = "Remove";
                }
                else {
                    (sender as Button).Content = "Download";
                }
            }
        }

        private async void DownloadModel_Click(object sender, RoutedEventArgs e) {
            bool isDownloaded = false;
            string model = string.Empty;
            (sender as Button).IsEnabled = false;
            try {
                model = (sender as Button).Tag.ToString();
                isDownloaded = _modelHelper.CheckModelState(model);
            }
            finally {
                if (isDownloaded) {
                    _modelHelper.DeleteModel(model);
                }
                else {
                    await _modelHelper.DownloadModel(model);
                }
            }
            (sender as Button).IsEnabled = true;
            DownloadModel_Loaded(sender, e);
        }
    }
}

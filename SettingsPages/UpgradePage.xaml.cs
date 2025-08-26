using Fairmark.Controls;
using Fairmark.Converters;
using Fairmark.Helpers;
using System;
using System.ComponentModel; // add this
using System.Diagnostics;
using Windows.Services.Store;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fairmark.SettingsPages {
    public sealed partial class UpgradePage : Page, INotifyPropertyChanged {
        private string _plusPrice = "...";
        private string _prevPrice = "...";
        private bool _isPromo;

        public string PlusPrice {
            get => _plusPrice;
            set { _plusPrice = value; OnPropertyChanged(nameof(PlusPrice)); }
        }

        public string PrevPrice {
            get => _prevPrice;
            set { _prevPrice = value; OnPropertyChanged(nameof(PrevPrice)); }
        }

        public bool IsPromo {
            get => _isPromo;
            set { _isPromo = value; OnPropertyChanged(nameof(IsPromo)); }
        }

        public UpgradePage() {
            this.InitializeComponent();
            DataContext = this;

        }

        private async void GlamButton_Loaded(object sender, RoutedEventArgs e) {
            (sender as GlamButton).IsEnabled = Variables.useStoreFeatures;
            IsPromo = await Variables.CheckForPromoAsync();
            (PlusPrice, PrevPrice) = await Variables.GetPricesAsync();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private async void GlamButton_Click(object sender, RoutedEventArgs e) {
            var mainWindow = Windows.UI.Xaml.Window.Current;
            if (mainWindow != null) {
                var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                await appView.TryEnterViewModeAsync(Windows.UI.ViewManagement.ApplicationViewMode.Default);
            }
            StorePurchaseStatus status = await Variables.PurchaseAsync();
            Debug.WriteLine($"[Store status] Purchase status: {status}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Services.Store;
using Windows.Storage;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace Fairmark.Helpers
{
    public static class Variables
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static bool useStoreFeatures = false;
#if !DEBUG
        public static bool firstStartup => localSettings.Values.ContainsKey("firstStartup") ? (bool)localSettings.Values["firstStartup"] : true;
#else
        public static bool firstStartup = false;
#endif
#if !DEBUG
        public static bool secondStartup => localSettings.Values.ContainsKey("secondStartup") && (bool)localSettings.Values["secondStartup"];
#else
        public static bool secondStartup = false;
#endif
        public static event EventHandler PlusStatusChanged;
        private static StoreContext context;
        public static async Task<bool> CheckIfPlusAsync()
        {
            if (useStoreFeatures) {
                if (context == null) {
                    context = StoreContext.GetDefault();
                }

                string[] productKinds = { "Durable" };
                var filterList = new List<string>(productKinds);

                StoreAppLicense license = await context.GetAppLicenseAsync();
                if (license == null) { return false; }

                string productId = "9P11H6Q5KQCQ";
                foreach (var prod in license.AddOnLicenses) {
                    if (prod.Key.StartsWith(productId) && prod.Value.IsActive)
                        return true;
                }
                return false;
            }
            else {
                if (Application.Current.Resources["Settings"] is Helpers.Settings settings) {
                    settings.PropertyChanged += (sender, e) => {
                        if (e.PropertyName == nameof(settings.DebugPlus)) {
                            Variables.PlusStatusChanged?.Invoke(null, EventArgs.Empty);
                        }
                    };
                }
                return (Application.Current.Resources["Settings"] as Settings).DebugPlus;
            }
        }

        public static async Task<string> GetPlusPriceAsync()
        {
            if (useStoreFeatures == false)
            {
                return "...";
            }
            if (context == null)
            {
                context = StoreContext.GetDefault();
            }
            var productKinds = new List<string> { "Durable" };

            StoreProductQueryResult result = await context.GetStoreProductsAsync(productKinds, new List<string> { "9P11H6Q5KQCQ" });

            if (result.ExtendedError != null)
            {
                Debug.WriteLine($"[Store status] Error: {result.ExtendedError.Message}");
            }

            if (result.Products.TryGetValue("9P11H6Q5KQCQ", out StoreProduct product))
            {
                return product.Price.FormattedPrice;
            }

            return "...";
        }
        public static string exportFolder => ApplicationData.Current.LocalFolder.Path;

    }
}

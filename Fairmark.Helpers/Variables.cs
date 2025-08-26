using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Services.Store;
using Windows.Storage;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace Fairmark.Helpers
{
    public static class Variables
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static bool useStoreFeatures = true;

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

        // Custom event delegate
        public delegate void PlusStatusChangedEventHandler(StorePurchaseStatus status);
        public static event PlusStatusChangedEventHandler PlusStatusChanged;

        private static StoreContext context;

        // Always use Store for Plus state
        public static async Task<bool> CheckIfPlusAsync()
        {
            if (!useStoreFeatures) return false;
            if (context == null) {
                context = StoreContext.GetDefault();
            }
            StoreAppLicense license = await context.GetAppLicenseAsync();
            if (license == null) { return false; }
            string productId = "9P11H6Q5KQCQ";
            foreach (var prod in license.AddOnLicenses) {
                if (prod.Key.StartsWith(productId) && prod.Value.IsActive)
                    return true;
            }
            return false;
        }

        // Get Plus price from Store
        public static async Task<string> GetPlusPriceAsync()
        {
            if (!useStoreFeatures)
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

        public static async Task<bool> CheckForPromoAsync() {
            if (!useStoreFeatures) return false;
            if (context == null) context = StoreContext.GetDefault();
            var productKinds = new List<string> { "Durable" };
            StoreProductQueryResult result = await context.GetStoreProductsAsync(productKinds, new List<string> { "9P11H6Q5KQCQ" });
            if (result.Products.TryGetValue("9P11H6Q5KQCQ", out StoreProduct product)) {
                // Promo is active if FormattedBasePrice != FormattedPrice
                return product.Price != null && product.Price.FormattedBasePrice != product.Price.FormattedPrice;
            }
            return false;
        }

        // Get both sale and regular prices
        public static async Task<(string PlusPrice, string PrevPrice)> GetPricesAsync() {
            string price = await GetPlusPriceAsync();
            string prev = string.Empty;
            if (useStoreFeatures && context == null) context = StoreContext.GetDefault();
            if (useStoreFeatures) {
                var productKinds = new List<string> { "Durable" };
                StoreProductQueryResult result = await context.GetStoreProductsAsync(productKinds, new List<string> { "9P11H6Q5KQCQ" });
                if (result.Products.TryGetValue("9P11H6Q5KQCQ", out StoreProduct product)) {
                    if (product.Price != null && product.Price.FormattedBasePrice != product.Price.FormattedPrice) {
                        prev = product.Price.FormattedBasePrice;
                    }
                }
            }
            return (price, prev);
        }

        public static string exportFolder => ApplicationData.Current.LocalFolder.Path;

        public static async Task<StorePurchaseStatus> PurchaseAsync() {
            var productId = "9P11H6Q5KQCQ";
            var context = Windows.Services.Store.StoreContext.GetDefault();
            var result = await context.RequestPurchaseAsync(productId);
            if (result.ExtendedError != null) {
                Debug.WriteLine($"Purchase error: {result.ExtendedError.Message}");
            }
            if (result.Status == StorePurchaseStatus.Succeeded || result.Status == StorePurchaseStatus.AlreadyPurchased) {
                PlusStatusChanged?.Invoke(result.Status);
            }
            return result.Status;
        }
    }
}

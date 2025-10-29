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

        public static string exportFolder => ApplicationData.Current.LocalFolder.Path;
    }
}

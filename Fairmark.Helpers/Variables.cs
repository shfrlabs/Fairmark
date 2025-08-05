using Windows.Storage;

namespace Fairmark.Helpers
{
    public static class Variables
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
#if !DEBUG
        public static bool firstStartup => localSettings.Values.ContainsKey("firstStartup") ? (bool)localSettings.Values["firstStartup"] : true;
#else
        public static bool firstStartup => false;
#endif

        public static string exportFolder => ApplicationData.Current.LocalFolder.Path;

    }
}

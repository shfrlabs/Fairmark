using Fairmark.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fairmark
{
    public sealed partial class OOBEPage : Page
    {
        public bool permsSet = false;
        public OOBEPage()
        {
            this.InitializeComponent();
        }

        private async void SetFolder_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");

            StorageFolder folder = await picker.PickSingleFolderAsync();

            if (folder != null)
            {
                Variables.vaultFolder = folder.Path;
                string token = Guid.NewGuid().ToString();
                Variables.folderAccessToken = token;
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, folder);
                Window.Current.Close();
            }
        }
    }
}

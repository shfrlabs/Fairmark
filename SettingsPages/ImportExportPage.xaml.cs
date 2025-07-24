using Fairmark.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
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
    public sealed partial class ImportExportPage : Page {
        public ImportExportPage() {
            this.InitializeComponent();
        }

        private async void ExportVault_Click(object sender, RoutedEventArgs e) {
            try {
                // 1. Generate the ZIP backup
                StorageFile backupFile = await NoteFileHandlingHelper.GetVaultBackupFileAsync();

                // 2. Let the user pick where to save it
                var savePicker = new FileSavePicker {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = "VaultBackup"
                };
                savePicker.FileTypeChoices.Add("Fairmark vault backup", new List<string> { ".fmbkp" });

                StorageFile destination = await savePicker.PickSaveFileAsync();
                if (destination == null) {
                    return;
                }
                await backupFile.CopyAndReplaceAsync(destination);
            }
            catch {
            }
        }

        private async void ImportVault_Click(object sender, RoutedEventArgs e) {
            try {
                var openPicker = new FileOpenPicker {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                openPicker.FileTypeFilter.Add(".fmbkp");

                StorageFile destination = await openPicker.PickSingleFileAsync();
                if (destination == null) {
                    return;
                }
                await NoteFileHandlingHelper.RestoreNotes(destination);
            }
            catch {
            }
        }
    }
}

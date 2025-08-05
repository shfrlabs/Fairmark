using Fairmark.Helpers;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Fairmark.SettingsPages
{
    public sealed partial class ImportExportPage : Page
    {
        public ImportExportPage()
        {
            this.InitializeComponent();
        }

        private async void ExportVault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFile backupFile = await NoteFileHandlingHelper.GetVaultBackupFileAsync();

                var savePicker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = "VaultBackup"
                };
                savePicker.FileTypeChoices.Add("Fairmark vault backup", new List<string> { ".fmbkp" });

                StorageFile destination = await savePicker.PickSaveFileAsync();
                if (destination == null)
                {
                    return;
                }
                await backupFile.CopyAndReplaceAsync(destination);
            }
            catch
            {
            }
        }

        private async void ImportVault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openPicker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                openPicker.FileTypeFilter.Add(".fmbkp");

                StorageFile destination = await openPicker.PickSingleFileAsync();
                if (destination == null)
                {
                    return;
                }
                _ = await NoteFileHandlingHelper.RestoreNotes(destination);
            }
            catch
            {
            }
        }
    }
}

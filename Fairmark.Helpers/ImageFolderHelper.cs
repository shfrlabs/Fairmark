using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.Helpers
{
    public class ImageFolderHelper
    {
        private string imageFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Images");
        private async Task<bool> Initialize()
        {
            if (!Directory.Exists(imageFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(imageFolderPath);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating image folder: {ex.Message}");
                    return false;
                }
            }
            else
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(imageFolderPath);
                    if (folder != null)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error accessing image folder: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> ImportImage(StorageFile[] files)
        {
            if (await Initialize())
            {
                foreach (StorageFile file in files)
                {
                    if (file != null)
                    {
                        string name = file.Name;
                        try
                        {
                            var folder = await StorageFolder.GetFolderFromPathAsync(imageFolderPath);
                            try
                            {
                                await folder.GetFileAsync(file.Name);
                                name = await GetUniqueFileName(file.Name);
                            }
                            catch (FileNotFoundException)
                            {
                            }
                            var destinationFile = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
                            await file.CopyAndReplaceAsync(destinationFile);
                            Debug.WriteLine($"Image {name} imported successfully.");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error importing image {name}: {ex.Message}");
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> DeleteImage(string fileName)
        {
            if (await Initialize())
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(imageFolderPath);
                    var file = await folder.GetFileAsync(fileName);
                    if (file != null)
                    {
                        await file.DeleteAsync();
                        Debug.WriteLine($"Image {fileName} deleted successfully.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deleting image {fileName}: {ex.Message}");
                }
            }
            return false;
        }

        public async Task<List<string>> GetImageList()
        {
            List<string> imageList = new List<string>();
            if (await Initialize())
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(imageFolderPath);
                    var files = await folder.GetFilesAsync();
                    foreach (var file in files)
                    {
                        imageList.Add(file.Name);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error retrieving image list: {ex.Message}");
                }
            }
            return imageList;
        }

        private async Task<string> GetUniqueFileName(string name)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(imageFolderPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(name);
            string uniqueName = name;
            int copyIndex = 1;

            while (true)
            {
                try
                {
                    await folder.GetFileAsync(uniqueName);
                    uniqueName = $"{fileNameWithoutExtension} (copy{(copyIndex > 1 ? $" {copyIndex}" : "")}){extension}";
                    copyIndex++;
                }
                catch (FileNotFoundException)
                {
                    break;
                }
            }
            return uniqueName;
        }
    }
}

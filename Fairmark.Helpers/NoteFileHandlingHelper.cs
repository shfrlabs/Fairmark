using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.Helpers
{
    public static class NoteFileHandlingHelper
    {
        public static async Task<bool> NoteExists(string noteId)
        {
            return (await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).TryGetItemAsync(noteId + ".md")) != null;
        }

        public static async Task<string> ReadNoteFileAsync(string noteId)
        {
            var file = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).TryGetItemAsync(noteId + ".md") as StorageFile;
            if (file != null)
            {
                return await FileIO.ReadTextAsync(file);
            }
            return null;
        }

        public static async Task WriteNoteFileAsync(string noteId, string content)
        {
            var file = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).CreateFileAsync(noteId + ".md", CreationCollisionOption.OpenIfExists);
            try
            {
                await RunWithLockAsync(noteId, async () =>
                {
                    await FileIO.WriteTextAsync(file, content);
                });
            }
            catch (System.IO.IOException)
            {
                return;
            }
        }

        public static async Task<StorageFile> GetVaultBackupFileAsync()
        {
            var localFolder = (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists));

            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var zipFile = await tempFolder
                .CreateFileAsync("VaultBackup.fmbkp", CreationCollisionOption.ReplaceExisting);
            using (var zipStream = await zipFile.OpenAsync(FileAccessMode.ReadWrite))
            using (var outStream = zipStream.GetOutputStreamAt(0))
            using (var archiveStream = outStream.AsStreamForWrite())
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: false))
            {
                var items = await localFolder.GetItemsAsync();
                var itemsList = items.ToList();
                _ = itemsList.RemoveAll(i => i.Name == "Fairmark.log");
                var files = itemsList.OfType<StorageFile>();
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(file.Name, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    using (var fileStream = await file.OpenStreamForReadAsync())
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            return zipFile;
        }

        public static async Task<bool> RestoreNotes(StorageFile backup)
        {
            if (!backup.FileType.Contains("fmbkp", StringComparison.OrdinalIgnoreCase)
                || !backup.IsAvailable)
            {
                return false;
            }
            var tempFolder = await ApplicationData.Current.TemporaryFolder
                .CreateFolderAsync("FairmarkRestore", CreationCollisionOption.ReplaceExisting);

            using (var zipStreamRef = await backup.OpenAsync(FileAccessMode.Read))
            using (var zipStream = zipStreamRef.AsStreamForRead())
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }

                    var dest = await tempFolder.CreateFileAsync(
                        entry.FullName,
                        CreationCollisionOption.ReplaceExisting);

                    using (var entryStream = entry.Open())
                    using (var fileStream = await dest.OpenStreamForWriteAsync())
                    {
                        await entryStream.CopyToAsync(fileStream);
                    }
                }
            }

            try
            {
                var jsonFile = await tempFolder.GetFileAsync("notes.json");
                var jsonText = await FileIO.ReadTextAsync(jsonFile);

                var noteList = JsonSerializer.Deserialize<List<NoteMetadata>>(jsonText);
                if (noteList == null)
                {
                    return false;
                }

                var notesFolder = (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists));

                foreach (var meta in noteList)
                {
                    meta.Tags.Clear();

                    var oldId = meta.Id;
                    meta.Id = Guid.NewGuid().ToString();

                    StorageFile extractedMd;
                    try
                    {
                        extractedMd = await tempFolder.GetFileAsync($"{oldId}.md");
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    await extractedMd.RenameAsync($"{meta.Id}.md", NameCollisionOption.ReplaceExisting);

                    _ = await extractedMd.CopyAsync(
                        notesFolder,
                        $"{meta.Id}.md",
                        NameCollisionOption.ReplaceExisting);

                    if (await NoteFileHandlingHelper.NoteExists(meta.Id))
                    {
                        NoteCollectionHelper.notes.Add(meta);
                    }
                }
                await NoteCollectionHelper.SaveNotes();
                return true;
            }
            catch
            {
                return false;
            }

        }



        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.SemaphoreSlim> _locks =
            new System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.SemaphoreSlim>();

        internal static async Task RunWithLockAsync(string noteId, Func<Task> action)
        {
            var semaphore = _locks.GetOrAdd(noteId, _ => new System.Threading.SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                _ = semaphore.Release();
            }
        }
    }
}
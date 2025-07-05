using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.Helpers {
    public static class NoteFileHandlingHelper {
        public static async Task<bool> NoteExists(string noteId) {
            return (await ApplicationData.Current.LocalFolder.TryGetItemAsync(noteId + ".md")) != null;
        }

        public static async Task<string> ReadNoteFileAsync(string noteId) {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(noteId + ".md") as StorageFile;
            if (file != null) {
                return await FileIO.ReadTextAsync(file);
            }
            return null;
        }

        public static async Task WriteNoteFileAsync(string noteId, string content) {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(noteId + ".md", CreationCollisionOption.OpenIfExists);
            try {
                await RunWithLockAsync(noteId, async () => {
                    await FileIO.WriteTextAsync(file, content);
                });
            }
            catch (System.IO.IOException) {
                return;
            }
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.SemaphoreSlim> _locks =
            new System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.SemaphoreSlim>();

        internal static async Task RunWithLockAsync(string noteId, Func<Task> action) {
            var semaphore = _locks.GetOrAdd(noteId, _ => new System.Threading.SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try {
                await action();
            }
            finally {
                semaphore.Release();
            }
        }
    }
}
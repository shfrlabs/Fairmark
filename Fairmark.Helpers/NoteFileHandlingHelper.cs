using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;

namespace Fairmark.Helpers {
    public static class NoteFileHandlingHelper {
        public static async Task<bool> NoteExists(string noteId) {
            return (await ApplicationData.Current.LocalFolder.TryGetItemAsync(noteId + ".rtf")) != null;
        }

        public static async Task<IRandomAccessStream> ReadNoteStreamAsync(string noteId) {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(noteId + ".rtf") as StorageFile;
            if (file != null) {
                return await file.OpenAsync(FileAccessMode.Read);
            }
            return null;
        }

        public static async Task WriteNoteFileAsync(string noteId, RichEditTextDocument document) {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(noteId + ".rtf", CreationCollisionOption.OpenIfExists);
            try {
                await RunWithLockAsync(noteId, async () => {
                    using (var outStream = await file.OpenAsync(FileAccessMode.ReadWrite)) {
                        outStream.Size = 0;
                        document.SaveToStream(TextGetOptions.FormatRtf, outStream);
                    }
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
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.Helpers {
    public static class NoteFileHandlingHelper {
        public static async Task<bool> NoteExists(string noteId) {
            return (await ApplicationData.Current.LocalFolder.TryGetItemAsync(noteId + ".md")) != null;
        }
    }
}
using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.Helpers
{
    public static class NoteCollectionHelper
    {
        public static ObservableCollection<NoteMetadata> notes = new ObservableCollection<NoteMetadata>();
        public static ObservableCollection<NoteTag> tags = new ObservableCollection<NoteTag>();

        public static async Task Initialize()
        {
            StorageFile notefile = await ApplicationData.Current.LocalFolder.CreateFileAsync("notes.json", CreationCollisionOption.OpenIfExists);
            StorageFile tagfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("tags.json", CreationCollisionOption.OpenIfExists);

            string noteContent = await FileIO.ReadTextAsync(notefile);
            string tagContent = await FileIO.ReadTextAsync(tagfile);

            try
            {
                var noteList = JsonSerializer.Deserialize<List<NoteMetadata>>(noteContent);
                if (noteList != null) {
                    foreach (var n in noteList) {
                        if (await NoteFileHandlingHelper.NoteExists(n.Id)) {
                            notes.Add(n);
                        }
                    }
                    await SaveNotes();
                }
            }
            catch { }

            try
            {
                JsonSerializer.Deserialize<List<NoteTag>>(tagContent).ForEach(t => tags.Add(t));
            }
            catch { }
        }

        public static async Task SaveNotes() {
            StorageFile notefile = await ApplicationData.Current.LocalFolder.CreateFileAsync("notes.json", CreationCollisionOption.OpenIfExists);
            string oldNoteContent = await FileIO.ReadTextAsync(notefile);
            List<NoteMetadata> oldNotes = null;
            try {
                oldNotes = JsonSerializer.Deserialize<List<NoteMetadata>>(oldNoteContent) ?? new List<NoteMetadata>();
            }
            catch {
                oldNotes = new List<NoteMetadata>();
            }

            var oldIds = new HashSet<string>();
            foreach (var n in oldNotes)
                oldIds.Add(n.Id);

            var newIds = new HashSet<string>();
            foreach (var n in notes)
                newIds.Add(n.Id);

            foreach (var id in newIds) {
                if (!oldIds.Contains(id)) {
                    await ApplicationData.Current.LocalFolder.CreateFileAsync($"{id}.md", CreationCollisionOption.OpenIfExists);
                }
            }

            foreach (var id in oldIds) {
                if (!newIds.Contains(id)) {
                    try {
                        StorageFile fileToDelete = await ApplicationData.Current.LocalFolder.GetFileAsync($"{id}.md");
                        await fileToDelete.DeleteAsync();
                    }
                    catch {
                        // File may not exist, ignore
                    }
                }
            }

            StorageFile saveFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("notes.json", CreationCollisionOption.ReplaceExisting);
            string noteContent = JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
            await FileIO.WriteTextAsync(saveFile, noteContent);
        }

        public static async Task SaveTags()
        {
            StorageFile tagfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("tags.json", CreationCollisionOption.ReplaceExisting);
            string tagContent = JsonSerializer.Serialize(tags, new JsonSerializerOptions { WriteIndented = true });
            await FileIO.WriteTextAsync(tagfile, tagContent);
        }
    }
}
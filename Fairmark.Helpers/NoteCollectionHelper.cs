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
                JsonSerializer.Deserialize<List<NoteMetadata>>(noteContent).ForEach(n => notes.Add(n));
            }
            catch { }

            try
            {
                JsonSerializer.Deserialize<List<NoteTag>>(tagContent).ForEach(t => tags.Add(t));
            }
            catch { }
        }

        public static async Task SaveNotes()
        {
            StorageFile notefile = await ApplicationData.Current.LocalFolder.CreateFileAsync("notes.json", CreationCollisionOption.ReplaceExisting);
            string noteContent = JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
            await FileIO.WriteTextAsync(notefile, noteContent);
        }

        public static async Task SaveTags()
        {
            StorageFile tagfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("tags.json", CreationCollisionOption.ReplaceExisting);
            string tagContent = JsonSerializer.Serialize(tags, new JsonSerializerOptions { WriteIndented = true });
            await FileIO.WriteTextAsync(tagfile, tagContent);
        }
    }
}
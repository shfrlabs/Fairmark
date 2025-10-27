using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace Fairmark.Helpers
{
    public static class NoteCollectionHelper
    {
        public static ObservableCollection<NoteGroup> groupedNotes = new ObservableCollection<NoteGroup>();
        public static ObservableCollection<NoteMetadata> notes = new ObservableCollection<NoteMetadata>();
        public static ObservableCollection<NoteTag> tags = new ObservableCollection<NoteTag>();


        public static async Task Initialize()
        {
            notes.CollectionChanged += (s, e) =>
            {
                Regroup();

                if (e.NewItems != null) {
                    foreach (NoteMetadata note in e.NewItems)
                        note.PropertyChanged += Note_PropertyChanged;
                }
                if (e.OldItems != null) {
                    foreach (NoteMetadata note in e.OldItems)
                        note.PropertyChanged -= Note_PropertyChanged;
                }
            };
            StorageFile notefile = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).CreateFileAsync("notes.json", CreationCollisionOption.OpenIfExists);
            StorageFile tagfile = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).CreateFileAsync("tags.json", CreationCollisionOption.OpenIfExists);

            string noteContent = await FileIO.ReadTextAsync(notefile);
            string tagContent = await FileIO.ReadTextAsync(tagfile);

            try
            {
                var noteList = JsonSerializer.Deserialize<List<NoteMetadata>>(noteContent);
                if (noteList != null)
                {
                    foreach (var n in noteList)
                    {
                        if (await NoteFileHandlingHelper.NoteExists(n.Id))
                        {
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
            foreach (var note in notes) { note.PropertyChanged += Note_PropertyChanged; note.ResolveTagGuids(guid => GetTagByGUID(guid)); }
            Debug.WriteLine($"notes: {notes.Count}, groupedNotes: {groupedNotes.Count}");

        }

        private static void Note_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        }


        public static void SortGroup(ObservableCollection<NoteMetadata> group, IEnumerable<NoteMetadata> sorted) {
            var sortedList = sorted.ToList();

            for (int i = group.Count - 1; i >= 0; i--) {
                if (!sortedList.Contains(group[i]))
                    group.RemoveAt(i);
            }

            foreach (var item in sortedList) {
                if (!group.Contains(item))
                    group.Add(item);
            }

            for (int targetIndex = 0; targetIndex < sortedList.Count; targetIndex++) {
                var item = sortedList[targetIndex];
                int currentIndex = group.IndexOf(item);
                if (currentIndex != targetIndex)
                    group.Move(currentIndex, targetIndex);
            }
        }


        public static void Regroup(IEnumerable<NoteMetadata> sequence = null) {
            if (sequence == null)
                sequence = notes;

            var pinned = sequence.Where(n => n.IsPinned).ToList();
            var others = sequence.Where(n => !n.IsPinned).ToList();

            NoteGroup pinnedGroup = groupedNotes.FirstOrDefault(g => g.Key == "PinnedGroup");
            if (pinnedGroup == null) {
                pinnedGroup = new NoteGroup("PinnedGroup");
                groupedNotes.Insert(0, pinnedGroup);
            }
            SortGroup(pinnedGroup, pinned);

            NoteGroup othersGroup = groupedNotes.FirstOrDefault(g => g.Key == "OthersGroup");
            if (othersGroup == null) {
                othersGroup = new NoteGroup("OthersGroup");
                groupedNotes.Add(othersGroup);
            }
            SortGroup(othersGroup, others);
        }


        public static void ApplySort(IEnumerable<NoteMetadata> sorted) {
            var sortedList = sorted.ToList();

            for (int targetIndex = 0; targetIndex < sortedList.Count; targetIndex++) {
                var item = sortedList[targetIndex];
                int currentIndex = notes.IndexOf(item);

                if (currentIndex != targetIndex) {
                    notes.Move(currentIndex, targetIndex);
                }
            }
        }

        public static async Task SaveNotes()
        {
            StorageFile notefile = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).CreateFileAsync("notes.json", CreationCollisionOption.OpenIfExists);
            string oldNoteContent = await FileIO.ReadTextAsync(notefile);
            List<NoteMetadata> oldNotes;
            try
            {
                oldNotes = JsonSerializer.Deserialize<List<NoteMetadata>>(oldNoteContent) ?? new List<NoteMetadata>();
            }
            catch
            {
                oldNotes = new List<NoteMetadata>();
            }

            var oldIds = new HashSet<string>();
            foreach (var n in oldNotes)
                _ = oldIds.Add(n.Id);

            var newIds = new HashSet<string>();
            foreach (var n in notes)
                _ = newIds.Add(n.Id);

            foreach (var id in newIds)
            {
                if (!oldIds.Contains(id))
                {
                    _ = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).CreateFileAsync($"{id}.md", CreationCollisionOption.OpenIfExists);
                }
            }

            foreach (var id in oldIds)
            {
                if (!newIds.Contains(id))
                {
                    try
                    {
                        StorageFile fileToDelete = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).GetFileAsync($"{id}.md");
                        await fileToDelete.DeleteAsync();
                    }
                    catch
                    {
                    }
                }
            }

            StorageFile saveFile = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).CreateFileAsync("notes.json", CreationCollisionOption.ReplaceExisting);
            string noteContent = JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
            await FileIO.WriteTextAsync(saveFile, noteContent);
        }

        public static async Task SaveTags()
        {
            StorageFile tagfile = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists)).CreateFileAsync("tags.json", CreationCollisionOption.ReplaceExisting);
            foreach (var note in notes)
            {
                foreach (var tag in note.Tags.ToList())
                {
                    if (!tags.Any(t => t.GUID == tag.GUID))
                    {
                        _ = note.Tags.Remove(tag);
                    }
                }
            }
            string tagContent = JsonSerializer.Serialize(tags, new JsonSerializerOptions { WriteIndented = true });
            await FileIO.WriteTextAsync(tagfile, tagContent);
        }

        public static NoteTag GetTagByGUID(string guid)
        {
            return tags.FirstOrDefault(t => t.GUID == guid);
        }
        public static async Task<(string Id, string Name, string Emoji, Color[] Colors, bool IsPinned, DateTimeOffset? LastModified)[]> GetNoteListAsync()
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("default", CreationCollisionOption.OpenIfExists);
                var noteFile = await folder.CreateFileAsync("notes.json", CreationCollisionOption.OpenIfExists);
                var tagFile = await folder.CreateFileAsync("tags.json", CreationCollisionOption.OpenIfExists);

                string noteContent = await FileIO.ReadTextAsync(noteFile);
                string tagContent = await FileIO.ReadTextAsync(tagFile);

                List<NoteMetadata> noteList;
                try { noteList = JsonSerializer.Deserialize<List<NoteMetadata>>(noteContent) ?? new List<NoteMetadata>(); }
                catch { noteList = new List<NoteMetadata>(); }

                List<NoteTag> tagList;
                try { tagList = JsonSerializer.Deserialize<List<NoteTag>>(tagContent) ?? new List<NoteTag>(); }
                catch { tagList = new List<NoteTag>(); }

                var tagDict = tagList.Where(t => t != null && t.GUID != null).ToDictionary(t => t.GUID, t => t);

                var result = await Task.WhenAll(noteList.Select(async n =>
                {
                    Color[] colors = new Color[0];
                    try
                    {
                        if (n?.TagGuids != null)
                        {
                            colors = n.TagGuids.Select(g => tagDict.ContainsKey(g) ? tagDict[g].Color : new Color()).ToArray();
                        }
                    }
                    catch { }

                    DateTimeOffset? lastModified = null;
                    try
                    {
                        lastModified = await NoteFileHandlingHelper.GetNoteLastModifiedAsync(n?.Id);
                    }
                    catch { }

                    return (n?.Id ?? string.Empty, n?.Name ?? string.Empty, n?.Emoji ?? string.Empty, colors, n?.IsPinned ?? false, lastModified);
                }));

                return result;
            }
            catch
            {
                return new (string, string, string, Color[], bool, DateTimeOffset?)[0];
            }
        }
    }
}
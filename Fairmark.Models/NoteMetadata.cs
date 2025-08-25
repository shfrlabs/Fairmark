using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Fairmark.Models {
    public class NoteMetadata : INotifyPropertyChanged {
        private string _id;
        private string _name;
        private string _emoji;
        private ObservableCollection<NoteTag> _tags;
        private bool _isPinned;

        public NoteMetadata() {
            _tags = new ObservableCollection<NoteTag>();
            _tags.CollectionChanged += Tags_CollectionChanged;
        }

        public string Id {
            get => _id;
            set {
                if (_id != value) {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Emoji {
            get => _emoji;
            set {
                if (_emoji != value) {
                    _emoji = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPinned {
            get => _isPinned;
            set {
                if (_isPinned != value) {
                    _isPinned = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public ObservableCollection<NoteTag> Tags {
            get => _tags;
            private set {
                if (_tags != value) {
                    // detach from old
                    if (_tags != null) {
                        _tags.CollectionChanged -= Tags_CollectionChanged;
                        foreach (var oldTag in _tags.ToList())
                            oldTag.PropertyChanged -= Tag_PropertyChanged;
                    }

                    _tags = value ?? new ObservableCollection<NoteTag>();

                    // attach to new
                    foreach (var newTag in _tags.ToList())
                        newTag.PropertyChanged += Tag_PropertyChanged;
                    _tags.CollectionChanged += Tags_CollectionChanged;

                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("Tags")]
        public List<string> TagGuids { get; set; } = new List<string>();

        public void ResolveTagGuids(Func<string, NoteTag> resolver) {
            if (resolver == null)
                return;

            var newTags = new ObservableCollection<NoteTag>();

            foreach (var guid in TagGuids ?? Enumerable.Empty<string>()) {
                var tag = resolver(guid);
                if (tag != null) {
                    newTags.Add(tag);
                    Debug.WriteLine("Loaded tag: " + guid + " " + tag.Name + " on noteID: " + Id);
                }
                    
            }

            Tags = newTags;
        }

        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (NoteTag tag in e.NewItems) {
                    tag.PropertyChanged += Tag_PropertyChanged;
                    if (!TagGuids.Contains(tag.GUID))
                        TagGuids.Add(tag.GUID);
                }
            }
            if (e.OldItems != null) {
                foreach (NoteTag tag in e.OldItems) {
                    tag.PropertyChanged -= Tag_PropertyChanged;
                    _ = TagGuids.Remove(tag.GUID);
                }
            }

            OnPropertyChanged(nameof(Tags));
        }


        private void Tag_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(NoteTag.Color)) {
                OnPropertyChanged(nameof(Tags));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

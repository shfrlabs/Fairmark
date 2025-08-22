using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace Fairmark.Models {
    public class NoteGroup : ObservableCollection<NoteMetadata> {
        public string Key { get; set; }

        private ResourceLoader loader = ResourceLoader.GetForCurrentView();
        private readonly System.ComponentModel.PropertyChangedEventArgs headerChangedArgs =
    new(nameof(HeaderName));

        public string HeaderName => loader.GetString(Key);

        public NoteGroup(string key, IEnumerable<NoteMetadata> items) : base(items) {
            Key = key;
            CollectionChanged += (s, e) => {
                OnPropertyChanged(headerChangedArgs);
            };
        }

        public NoteGroup(string key) : base() {
            Key = key;
            CollectionChanged += (s, e) => {
                OnPropertyChanged(headerChangedArgs);
            };
        }

    }
}

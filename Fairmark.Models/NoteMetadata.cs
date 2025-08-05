using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fairmark.Models
{
    public class NoteMetadata : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _emoji;
        private ObservableCollection<NoteTag> _tags;

        public NoteMetadata()
        {
            _tags = new ObservableCollection<NoteTag>();
            _tags.CollectionChanged += Tags_CollectionChanged;
        }

        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Emoji
        {
            get => _emoji;
            set
            {
                if (_emoji != value)
                {
                    _emoji = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<NoteTag> Tags
        {
            get => _tags;
            set
            {
                if (_tags != value)
                {
                    if (_tags != null)
                        _tags.CollectionChanged -= Tags_CollectionChanged;
                    _tags = value;
                    if (_tags != null)
                        _tags.CollectionChanged += Tags_CollectionChanged;
                    OnPropertyChanged();
                }
            }
        }

        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Tags));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
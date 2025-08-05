using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace Fairmark.Models
{
    public class NoteTag : INotifyPropertyChanged
    {
        private string _name;
        private string _emoji;
        private string _guid;
        private Color _color;

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

        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GUID
        {
            get => _guid;
            set
            {
                if (_guid != value)
                {
                    _guid = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
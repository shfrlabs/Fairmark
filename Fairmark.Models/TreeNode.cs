using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Fairmark.Models
{
    public class TreeNode : INotifyPropertyChanged
    {
        private Guid _id;
        private string _name;
        private bool _isFolder;
        private ObservableCollection<TreeNode> _children;

        public Guid Id
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

        public bool IsFolder
        {
            get => _isFolder;
            set
            {
                if (_isFolder != value)
                {
                    _isFolder = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Emoji));
                }
            }
        }

        public ObservableCollection<TreeNode> Children
        {
            get => _children;
            set
            {
                if (_children != value)
                {
                    _children = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Emoji => IsFolder ? "📁" : "📄";

        public TreeNode(string name, bool isFolder = false)
        {
            _id = Guid.NewGuid();
            _name = name;
            _isFolder = isFolder;
            _children = new ObservableCollection<TreeNode>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
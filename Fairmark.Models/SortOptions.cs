using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Fairmark.Models {
    public class SortOptions : INotifyPropertyChanged {
        private bool _sortDefault;
        private bool _sortTag;
        private bool _sortName;

        public bool sortDefault {
            get => _sortDefault;
            set {
                if (_sortDefault != value) {
                    _sortDefault = value;
                    if (value) {
                        sortTag = false;
                        sortName = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public bool sortTag {
            get => _sortTag;
            set {
                if (_sortTag != value) {
                    _sortTag = value;
                    if (value) {
                        sortDefault = false;
                        sortName = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public bool sortName {
            get => _sortName;
            set {
                if (_sortName != value) {
                    _sortName = value;
                    if (value) {
                        sortDefault = false;
                        sortTag = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

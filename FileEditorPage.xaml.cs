using Fairmark.Helpers;
using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fairmark
{
    public sealed partial class FileEditorPage : Page
    {
        public string noteId = null;
        public FileEditorPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            noteId = (e.Parameter as NoteMetadata).Id;
            MarkEditor.Text = await NoteFileHandlingHelper.ReadNoteFileAsync(noteId);
        }

        private void MarkEditor_TextChanged(object sender, TextChangedEventArgs e) {
            NoteFileHandlingHelper.WriteNoteFileAsync(noteId, MarkEditor.Text);
        }
    }
}

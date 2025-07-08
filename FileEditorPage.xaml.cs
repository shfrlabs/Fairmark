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
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fairmark
{
    public sealed partial class FileEditorPage : Page {
        public string noteId = null;
        public FileEditorPage() {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            noteId = (e.Parameter as NoteMetadata).Id;
            MarkEditor.Text = await NoteFileHandlingHelper.ReadNoteFileAsync(noteId);
        }

        private async void MarkEditor_TextChanged(object sender, TextChangedEventArgs e) {
            await NoteFileHandlingHelper.WriteNoteFileAsync(noteId, MarkEditor.Text);
        }

        public ApplicationView currentView => ApplicationView.GetForCurrentView();
        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e) {
            if (currentView.IsFullScreenMode) {
                currentView.ExitFullScreenMode();
            }
            else {
                currentView.TryEnterFullScreenMode();
            }
        }
    }
}

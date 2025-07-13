using Fairmark.Helpers;
using Fairmark.Models;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Reflection.Metadata.Ecma335;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fairmark {
    public sealed partial class FileEditorPage : Page {
        private string noteId;
        private bool _suppressNumberBoxValueChanged = false;

        public FileEditorPage() {
            InitializeComponent();
            MarkEditor.SelectionChanged += (s, e) => UpdateFormat();
            MarkEditor.KeyDown += (s, e) => UpdateFormat();
            MarkEditor.PreviewKeyDown += (s, e) => UpdateFormat();
            MarkEditor.TextChanged += (s, e) => UpdateFormat();
            MarkEditor.SelectionHighlightColorWhenNotFocused = MarkEditor.SelectionHighlightColor;
        }

        private void UpdateFormat()
        {
            _suppressNumberBoxValueChanged = true;
            var format = MarkEditor.Document.Selection.CharacterFormat;
            BoldToggle.IsChecked = format.Bold == FormatEffect.On;
            ItalicToggle.IsChecked = format.Italic == FormatEffect.On;
            UnderlineToggle.IsChecked = format.Underline == UnderlineType.Single;
            StrikeToggle.IsChecked = format.Strikethrough == FormatEffect.On;
            UndoButton.IsEnabled = MarkEditor.Document.CanUndo();
            RedoButton.IsEnabled = MarkEditor.Document.CanRedo();
            CutButton.IsEnabled = MarkEditor.Document.CanCopy();
            CopyButton.IsEnabled = MarkEditor.Document.CanCopy();
            PasteButton.IsEnabled = MarkEditor.Document.CanPaste();
            NumberBox.Value = (double)(EditorSizeConverter.Convert(MarkEditor.Document.Selection.CharacterFormat.Size, typeof(double), null, null));
            _suppressNumberBoxValueChanged = false;
        }

        public bool IsSelectionBold {
            get {
                var format = MarkEditor.Document.Selection.CharacterFormat;
                return format.Bold == Windows.UI.Text.FormatEffect.On;
            }
            set {
                var format = MarkEditor.Document.Selection.CharacterFormat;
                format.Bold = value ? Windows.UI.Text.FormatEffect.On : Windows.UI.Text.FormatEffect.Off;
            }
        }

        public bool IsSelectionItalic {
            get {
                var format = MarkEditor.Document.Selection.CharacterFormat;
                return format.Italic == Windows.UI.Text.FormatEffect.On;
            }
            set {
                var format = MarkEditor.Document.Selection.CharacterFormat;
                format.Italic = value ? Windows.UI.Text.FormatEffect.On : Windows.UI.Text.FormatEffect.Off;
            }
        }

        public bool IsSelectionUnderline {
            get {
                var format = MarkEditor.Document.Selection.CharacterFormat;
                return format.Underline == Windows.UI.Text.UnderlineType.Single;
            }
            set {
                var format = MarkEditor.Document.Selection.CharacterFormat;
                format.Underline = value ? Windows.UI.Text.UnderlineType.Single : Windows.UI.Text.UnderlineType.None;
            }
        }

        public bool IsSelectionStrikethrough {
            get {
                var format = MarkEditor.Document.Selection.CharacterFormat;
                return format.Strikethrough == Windows.UI.Text.FormatEffect.On;
            }
            set {
                var format = MarkEditor.Document.Selection.CharacterFormat;
                format.Strikethrough = value ? Windows.UI.Text.FormatEffect.On : Windows.UI.Text.FormatEffect.Off;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            noteId = (e.Parameter as NoteMetadata)?.Id;
            MarkEditor.Document.LoadFromStream(TextSetOptions.FormatRtf, await NoteFileHandlingHelper.ReadNoteStreamAsync(noteId));
        }

        public ApplicationView currentView => ApplicationView.GetForCurrentView();

        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e) {
            if (currentView.IsFullScreenMode)
                currentView.ExitFullScreenMode();
            else
                currentView.TryEnterFullScreenMode();
        }

        private async void MarkEditor_TextChanged(object sender, RoutedEventArgs e) {
            await NoteFileHandlingHelper.WriteNoteFileAsync(noteId, MarkEditor.TextDocument);
            
        }

        public bool CanUndo => MarkEditor.Document.CanUndo();
        public bool CanRedo => MarkEditor.Document.CanRedo();
        public bool CanCopy => MarkEditor.Document.CanCopy();
        public bool CanPaste => MarkEditor.Document.CanPaste();
        private void CopyButton_Click(object sender, RoutedEventArgs e) => MarkEditor.Document.Selection.Copy();
        private void CutButton_Click(object sender, RoutedEventArgs e) => MarkEditor.Document.Selection.Cut();
        private void PasteButton_Click(object sender, RoutedEventArgs e) => MarkEditor.Document.Selection.Paste(0);
        private void UndoButton_Click(object sender, RoutedEventArgs e) => MarkEditor.Document.Undo();
        private void RedoButton_Click(object sender, RoutedEventArgs e) => MarkEditor.Document.Redo();

        private void NumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args) {
            if (_suppressNumberBoxValueChanged)
                return;

            double value = sender.Value;

            if (double.IsNaN(value) || value < sender.Minimum || value == 0) {
                value = sender.Minimum;
                _suppressNumberBoxValueChanged = true;
                sender.Value = value;
                _suppressNumberBoxValueChanged = false;
            }
            else if (value > sender.Maximum) {
                value = sender.Maximum;
                _suppressNumberBoxValueChanged = true;
                sender.Value = value;
                _suppressNumberBoxValueChanged = false;
            }

            float finalSize = (float)value;
            MarkEditor.Document.Selection.CharacterFormat.Size = finalSize;
        }
    }
}
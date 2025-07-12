using Fairmark.Helpers;
using Fairmark.Models;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fairmark {
    public sealed partial class FileEditorPage : Page {
        private string noteId;
        private ScrollViewer editorScroll;
        private bool isSyncing;

        public FileEditorPage() {
            InitializeComponent();

            MarkEditor.Loaded += (s, e) => {
                editorScroll = FindDescendantScrollViewer(MarkEditor);
                if (editorScroll != null)
                    editorScroll.ViewChanged += EditorViewChanged;
            };

            previewsv.ViewChanged += PreviewViewChanged;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            noteId = (e.Parameter as NoteMetadata)?.Id;
            MarkEditor.Text = await NoteFileHandlingHelper.ReadNoteFileAsync(noteId);
        }

        private void EditorViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            if (isSyncing || editorScroll == null)
                return;
            isSyncing = true;

            double offset = editorScroll.VerticalOffset;
            double maxEditor = EditorMaxOffset;
            double target;

            if (offset <= 0) {
                target = 0;
            }
            else if (offset >= maxEditor) {
                target = PreviewMaxOffset;
            }
            else {
                var lines = Math.Round(offset / EditorLineHeight);
                target = Clamp(lines * PreviewLineHeight, 0, PreviewMaxOffset);
            }

            previewsv.ChangeView(null, target, null, disableAnimation: true);
            isSyncing = false;
        }

        private void PreviewViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            if (isSyncing || editorScroll == null)
                return;
            isSyncing = true;

            double offset = previewsv.VerticalOffset;
            double maxPrev = PreviewMaxOffset;
            double target;

            if (offset <= 0) {
                target = 0;
            }
            else if (offset >= maxPrev) {
                target = EditorMaxOffset;
            }
            else {
                var lines = Math.Round(offset / PreviewLineHeight);
                target = Clamp(lines * EditorLineHeight, 0, EditorMaxOffset);
            }

            editorScroll.ChangeView(null, target, null, disableAnimation: true);
            isSyncing = false;
        }
        private async void MarkEditor_TextChanged(object sender, TextChangedEventArgs e) {
            if (editorScroll != null) {
                EditorViewChanged(editorScroll, null);
            }
            await NoteFileHandlingHelper.WriteNoteFileAsync(noteId, MarkEditor.Text);
        }
        private double EditorLineHeight => MarkEditor.FontSize * 1.2;

        private double PreviewLineHeight {
            get {
                if (MarkBlock is MarkdownTextBlock tb)
                    return (!double.IsNaN(tb.ParagraphLineHeight) && tb.ParagraphLineHeight > 0)
                        ? tb.ParagraphLineHeight
                        : tb.FontSize * 1.2;

                return (previewsv.ViewportHeight / Math.Max(1, previewsv.ExtentHeight))
                       * previewsv.ExtentHeight;
            }
        }

        private double EditorMaxOffset =>
            Math.Max(0, editorScroll.ExtentHeight - editorScroll.ViewportHeight);

        private double PreviewMaxOffset =>
            Math.Max(0, previewsv.ExtentHeight - previewsv.ViewportHeight);

        private static double Clamp(double v, double min, double max) =>
            Math.Min(Math.Max(v, min), max);

        private static ScrollViewer FindDescendantScrollViewer(DependencyObject parent) {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is ScrollViewer sv)
                    return sv;
                var deeper = FindDescendantScrollViewer(child);
                if (deeper != null)
                    return deeper;
            }
            return null;
        }

        public ApplicationView currentView => ApplicationView.GetForCurrentView();

        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e) {
            if (currentView.IsFullScreenMode)
                currentView.ExitFullScreenMode();
            else
                currentView.TryEnterFullScreenMode();
        }
    }
}
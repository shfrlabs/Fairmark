using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.WinUI.Controls.MarkdownTextBlockRns;
using Fairmark.Helpers;
using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace Fairmark {
    public sealed partial class FileEditorPage : Page {
        private string noteId;

        public FileEditorPage() {
            InitializeComponent();
            MarkEditor.CursorLineChanged += MarkEditor_CursorLineChanged;
            (MarkBlock.DataContext as Settings).PropertyChanged += (s, a) => { FixHeaderSizes(); };
            FixHeaderSizes();
        }

        private void MarkEditor_CursorLineChanged(object sender, Controls.FileEditorBox.CursorLineChangedEventArgs e) {
            // Only process if we have valid line movement
            if (e.PreviousLineNumber == 0 || e.CurrentLineNumber == e.PreviousLineNumber)
                return;

            double scrollAmount = 0;
            string[] lines = MarkEditor.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineIndex = e.PreviousLineNumber - 1;

            if (lineIndex >= 0 && lineIndex < lines.Length) {
                string line = lines[lineIndex];

                // Calculate exact height for the line we're leaving
                if (line.StartsWith("# "))
                    scrollAmount = MarkBlock.Config.Themes.H1FontSize + MarkBlock.Config.Themes.H1Margin.Top;
                else if (line.StartsWith("## "))
                    scrollAmount = MarkBlock.Config.Themes.H2FontSize + MarkBlock.Config.Themes.H2Margin.Top;
                else if (line.StartsWith("### "))
                    scrollAmount = MarkBlock.Config.Themes.H3FontSize + MarkBlock.Config.Themes.H3Margin.Top;
                else if (line.StartsWith("#### "))
                    scrollAmount = MarkBlock.Config.Themes.H4FontSize + MarkBlock.Config.Themes.H4Margin.Top;
                else if (line.StartsWith("##### "))
                    scrollAmount = MarkBlock.Config.Themes.H5FontSize + MarkBlock.Config.Themes.H5Margin.Top;
                else if (line.StartsWith("###### "))
                    scrollAmount = MarkBlock.Config.Themes.H6FontSize + MarkBlock.Config.Themes.H6Margin.Top;
                else
                    scrollAmount = MarkBlock.FontSize;
            }

            // Apply scroll direction
            if (e.CurrentLineNumber > e.PreviousLineNumber) {
                previewsv.ChangeView(null, previewsv.VerticalOffset + scrollAmount, null);
            }
            else if (e.CurrentLineNumber < e.PreviousLineNumber) {
                previewsv.ChangeView(null, previewsv.VerticalOffset - scrollAmount, null);
            }
        }

        private void FixHeaderSizes() {
            Debug.WriteLine("HEADER SIZES FIXED");
            if (MarkBlock.Config == null) {
                MarkdownThemes theme = new MarkdownThemes();
                MarkdownConfig config = new MarkdownConfig();
                double baseSize = (MarkBlock.DataContext as Settings).PreviewFontSize;
                theme.H1FontSize = baseSize * 1.5;
                theme.H2FontSize = baseSize * 1.4;
                theme.H3FontSize = baseSize * 1.3;
                theme.H4FontSize = baseSize * 1.2;
                theme.H5FontSize = baseSize * 1.1;
                theme.H6FontSize = baseSize;
                config.Themes = theme;
                MarkBlock.Config = config;
            }
            else {
                double baseSize = (MarkBlock.DataContext as Settings).PreviewFontSize;
                MarkBlock.Config.Themes.H1FontSize = baseSize * 1.5;
                MarkBlock.Config.Themes.H2FontSize = baseSize * 1.4;
                MarkBlock.Config.Themes.H3FontSize = baseSize * 1.3;
                MarkBlock.Config.Themes.H4FontSize = baseSize * 1.2;
                MarkBlock.Config.Themes.H5FontSize = baseSize * 1.1;
                MarkBlock.Config.Themes.H6FontSize = baseSize;
            }
            string original = MarkBlock.Text;
            MarkBlock.Text = original + "\u200B";  // Add ZWSP
            MarkBlock.Text = original;             // Remove it
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            noteId = (e.Parameter as NoteMetadata)?.Id;
            MarkEditor.Text = await NoteFileHandlingHelper.ReadNoteFileAsync(noteId);
        }

        private double CountHeight(string text) {
            double height = 0;
            foreach (string line in Regex.Split(MarkEditor.Text, @"\r\n|\r|\n")) {
                if (line.StartsWith("# ")) { height += MarkBlock.Config.Themes.H1FontSize; height += MarkBlock.Config.Themes.H1Margin.Top; }
                else if (line.StartsWith("## ")) { height += MarkBlock.Config.Themes.H2FontSize; height += MarkBlock.Config.Themes.H2Margin.Top; }
                else if (line.StartsWith("### ")) { height += MarkBlock.Config.Themes.H3FontSize; height += MarkBlock.Config.Themes.H3Margin.Top; }
                else if (line.StartsWith("#### ")) { height += MarkBlock.Config.Themes.H4FontSize; height += MarkBlock.Config.Themes.H4Margin.Top; }
                else if (line.StartsWith("##### ")) { height += MarkBlock.Config.Themes.H5FontSize; height += MarkBlock.Config.Themes.H5Margin.Top; }
                else if (line.StartsWith("###### ")) { height += MarkBlock.Config.Themes.H6FontSize; height += MarkBlock.Config.Themes.H6Margin.Top; }
                else { height += MarkBlock.FontSize; }
            }
            return height;
        }

        public ApplicationView currentView => ApplicationView.GetForCurrentView();

        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e) {
            if (currentView.IsFullScreenMode)
                currentView.ExitFullScreenMode();
            else
                currentView.TryEnterFullScreenMode();
        }

        

        private void previewsv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {

        }
    }
}
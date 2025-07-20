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
        private double _totalPreviewHeight;
        private double _lastEditorOffset;
        private double _lastEditorHeight;
        private bool _isScrollSyncing;

        public FileEditorPage() {
            InitializeComponent();
            FixHeaderSizes();
            (Application.Current.Resources["Settings"] as Settings).PropertyChanged += (s, e) => { FixHeaderSizes(); };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            noteId = (e.Parameter as NoteMetadata)?.Id;
            MarkEditor.Text = await NoteFileHandlingHelper.ReadNoteFileAsync(noteId);
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
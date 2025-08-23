using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using CommunityToolkit.WinUI.Controls.MarkdownTextBlockRns;
using Fairmark.Helpers;
using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace Fairmark
{
    public sealed partial class FileEditorPage : Page
    {
        private string noteId;
        public ImageFolderHelper imageFolderHelper = new ImageFolderHelper();
        public FileEditorPage()
        {
            InitializeComponent();
            FixHeaderSizes();
            this.ActualThemeChanged += (s, e) => {
                FixHeaderSizes();
            };
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, e) =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    frame.RequestedTheme = e.Theme;
                    FixHeaderSizes();
                }
            };
            (Application.Current.Resources["Settings"] as Settings).PropertyChanged += (s, e) => { FixHeaderSizes(); };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            noteId = (e.Parameter as NoteMetadata)?.Id;
            App.LogHelper.WriteLog($"Opened {noteId}, Name: '{(e.Parameter as NoteMetadata)?.Name}'");
            MarkEditor.Text = await NoteFileHandlingHelper.ReadNoteFileAsync(noteId);
        }

        private void FixHeaderSizes()
        {
            Debug.WriteLine("Header sizes fixed");
            if (MarkBlock.Config == null)
            {
                MarkdownThemes theme = new MarkdownThemes();
                MarkdownConfig config = new MarkdownConfig() { ImageProvider = new LocalImageProviderHelper() };
                double baseSize = (MarkBlock.DataContext as Settings).PreviewFontSize;
                theme.H1FontSize = baseSize * 1.5;
                theme.H2FontSize = baseSize * 1.4;
                theme.H3FontSize = baseSize * 1.3;
                theme.H4FontSize = baseSize * 1.2;
                theme.H5FontSize = baseSize * 1.1;
                theme.H6FontSize = baseSize;
                theme.HeadingForeground = MarkBlock.Foreground;
                config.Themes = theme;
                MarkBlock.Config = config;
            }
            else
            {
                double baseSize = (MarkBlock.DataContext as Settings).PreviewFontSize;
                MarkBlock.Config.Themes.H1FontSize = baseSize * 1.5;
                MarkBlock.Config.Themes.H2FontSize = baseSize * 1.4;
                MarkBlock.Config.Themes.H3FontSize = baseSize * 1.3;
                MarkBlock.Config.Themes.H4FontSize = baseSize * 1.2;
                MarkBlock.Config.Themes.H5FontSize = baseSize * 1.1;
                MarkBlock.Config.Themes.H6FontSize = baseSize;
                MarkBlock.Config.Themes.HeadingForeground = MarkBlock.Foreground;
            }

            string original = MarkBlock.Text;
            MarkBlock.Text = original + "\u200B";
            MarkBlock.Text = original;
        }

        public ApplicationView currentView => ApplicationView.GetForCurrentView();

        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentView.IsFullScreenMode)
                currentView.ExitFullScreenMode();
            else
                _ = currentView.TryEnterFullScreenMode();
        }

        private void previewsv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void InsertLink_Click(object sender, RoutedEventArgs e)
        {
            MarkEditor.InsertLink(LinkUrl.Text, LinkText.Text);
            LinkUrl.Text = string.Empty;
            LinkText.Text = string.Empty;
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            MarkEditor.InsertDetails(SummaryText.Text, DetailsText.Text);
            SummaryText.Text = string.Empty;
            DetailsText.Text = string.Empty;
        }

        private async void ImportImage_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            };
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            FilePickerSelectedFilesArray pickedfiles = (FilePickerSelectedFilesArray)await picker.PickMultipleFilesAsync();
            StorageFile[] picked = pickedfiles.ToArray();
            Debug.WriteLine($"Picked {picked.Length} files for import.");
            if (picked.Length > 0)
            {
                foreach (var file in picked)
                {
                    Debug.WriteLine($"Importing image: {file.Name}");
                    if (await imageFolderHelper.ImportImage(file))
                    {
                        Debug.WriteLine($"Image {file.Name} imported successfully.");
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to import image {file.Name}.");
                    }
                }
            }
            Images.ItemsSource = await imageFolderHelper.GetImageList();
        }

        private async void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            if (Images.SelectedItem != null)
            {
                StorageFile selectedImage = Images.SelectedItem as StorageFile;
                if (selectedImage != null)
                {
                    await MarkEditor.InsertImage(selectedImage.Name);
                }
            }
        }

        private async void Images_Loaded(object sender, RoutedEventArgs e)
        {
            Images.ItemsSource = await imageFolderHelper.GetImageList();
        }

        private async void DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            await imageFolderHelper.DeleteImage(((sender as AppBarButton).Tag as StorageFile).Name);
            Images.ItemsSource = await imageFolderHelper.GetImageList();
        }

        private void RefNote_Click(object sender, RoutedEventArgs e) {
            if (ReferenceList.SelectedItem != null) {
                NoteMetadata selectedNote = ReferenceList.SelectedItem as NoteMetadata;
                if (selectedNote != null) {
                    MarkEditor.InsertLink($"fairmark://default/{selectedNote.Id}", selectedNote.Name);
                }
            }
        }

        private void ReferenceList_Loaded(object sender, RoutedEventArgs e) {
            ReferenceList.ItemsSource = NoteCollectionHelper.notes.ToList();
        }

        private void RefFlyout_Click(object sender, RoutedEventArgs e) {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
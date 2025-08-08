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
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, e) =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    frame.RequestedTheme = e.Theme;
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
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            MarkEditor.InsertDetails(SummaryText.Text, DetailsText.Text);
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
            StorageFile[] picked = (await picker.PickMultipleFilesAsync()).ToArray();
            if (picked.Length > 0)
            {
                bool result = await imageFolderHelper.ImportImage(picked);
                if (result)
                {
                    Images.ItemsSource = await imageFolderHelper.GetImageList();
                }
                else
                {
                    Debug.WriteLine("Failed to import images.");
                }
            }
            Images.ItemsSource = await imageFolderHelper.GetImageList();
        }

        private async void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            if (Images.SelectedItem != null)
            {
                string selectedImage = Images.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedImage))
                {
                    await MarkEditor.InsertImage(selectedImage);
                }
            }
        }

        private void ImageSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private async void Images_Loaded(object sender, RoutedEventArgs e)
        {
            Images.ItemsSource = await imageFolderHelper.GetImageList();
        }
    }
}
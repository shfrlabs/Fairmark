using Fairmark.Helpers;
using Fairmark.Models;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Fairmark.SettingsPages
{
    public sealed partial class TagManagerPage : Page
    {
        public TagManagerPage()
        {
            this.InitializeComponent();
            RenameTag.IsEnabled = false;
            DeleteTag.IsEnabled = false;
            RecolorTag.IsEnabled = false;
            EmojiTag.IsEnabled = false;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagList.SelectedItem != null)
            {
                RenameTag.IsEnabled = true;
                DeleteTag.IsEnabled = true;
                RecolorTag.IsEnabled = true;
                EmojiTag.IsEnabled = true;
            }
            else
            {
                RenameTag.IsEnabled = false;
                DeleteTag.IsEnabled = false;
                RecolorTag.IsEnabled = false;
                EmojiTag.IsEnabled = false;
            }
        }

        private void RenameTag_Click(object sender, RoutedEventArgs e)
        {
            if (TagList.SelectedItem is not NoteTag tag) return;

            Flyout flyout = new Flyout();
            TextBox box = new TextBox
            {
                Text = tag.Name,
                MaxWidth = 300,
                MinWidth = 200,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(-5),
                FontSize = 13,
                CornerRadius = new CornerRadius(4),
                IsReadOnly = false
            };
            box.KeyDown += async (s, args) =>
            {
                if (args.Key == Windows.System.VirtualKey.Enter)
                {
                    if (!string.IsNullOrWhiteSpace(box.Text))
                    {
                        tag.Name = box.Text;
                        await NoteCollectionHelper.SaveTags();
                        flyout.Hide();
                    }
                    else
                    {
                        box.PlaceholderText = "Enter a name.";
                    }
                }
            };
            flyout.Content = box;
            flyout.ShowAt(sender as FrameworkElement);
        }

        private async void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            if (TagList.SelectedItem == null)
                return;

            NoteTag tag2remove = TagList.SelectedItem as NoteTag;
            _ = NoteCollectionHelper.tags.Remove(NoteCollectionHelper.tags.Where(t => t.GUID == tag2remove.GUID).First());
            App.LogHelper.WriteLog($"Tag {tag2remove.GUID} with name '{tag2remove.Name}' removed via tag manager.");
            await NoteCollectionHelper.SaveTags();
        }

        private void RecolorTag_Click(object sender, RoutedEventArgs e)
        {
            if (TagList.SelectedItem is not NoteTag tag) return;

            Flyout flyout = new Flyout();
            ColorPicker picker = new ColorPicker
            {
                ColorSpectrumShape = ColorSpectrumShape.Ring,
                Color = tag.Color
            };
            picker.ColorChanged += (s, args) =>
            {
                tag.Color = picker.Color;
            };
            flyout.Closed += async (s, args) =>
            {
                await NoteCollectionHelper.SaveTags();
            };
            flyout.Content = picker;
            flyout.ShowAt(sender as FrameworkElement);
        }

        private void EmojiTag_Click(object sender, RoutedEventArgs e)
        {
            if (TagList.SelectedItem is not NoteTag tag) return;

            Flyout flyout = new Flyout();
            StackPanel emojiPanel = new StackPanel { Orientation = Orientation.Vertical };
            var gridView = new GridView
            {
                ItemsPanel = (ItemsPanelTemplate)Application.Current.Resources["WrapGridPanel"],
                ItemTemplate = (DataTemplate)Application.Current.Resources["EmojiBlock"],
                ItemsSource = new EmojiHelper.IncrementalEmojiSource(),
                SelectionMode = ListViewSelectionMode.Single
            };
            gridView.SelectionChanged += async (s, args) =>
            {
                if (gridView.SelectedItem != null)
                {
                    tag.Emoji = gridView.SelectedItem.ToString();
                    await NoteCollectionHelper.SaveTags();
                    flyout.Hide();
                }
            };
            AutoSuggestBox searchBox = new AutoSuggestBox
            {
                PlaceholderText = "Search for an emoji...",
                Margin = new Thickness(0, 0, 0, 10),
                Width = 240,
                MaxWidth = 240,
                QueryIcon = new SymbolIcon(Symbol.Find)
            };
            searchBox.TextChanged += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    gridView.ItemsSource = new EmojiHelper.IncrementalEmojiSource();
                }
                else
                {
                    var searchTerm = searchBox.Text.ToLower();
                    gridView.ItemsSource = EmojiHelper.Emojis.Where(emoji =>
                        emoji.SearchTerms.Any(term => term.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }
            };
            emojiPanel.Children.Add(searchBox);
            emojiPanel.Children.Add(gridView);
            flyout.Content = emojiPanel;
            flyout.ShowAt(sender as FrameworkElement);
        }
    }
}

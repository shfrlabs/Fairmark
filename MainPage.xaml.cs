using Fairmark.Converters;
using Fairmark.Helpers;
using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Fairmark
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.SetTitleBar(DragRegion);
        }

        private NoteTag currentTag = null;
        private async Task WelcomeDialog()
        {
            ContentDialog welcomeDialog = new ContentDialog
            {
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Exit app",
                DefaultButton = ContentDialogButton.Primary
            };
            welcomeDialog.Content = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Orientation = Orientation.Vertical,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Welcome to Fairmark!",
                        FontSize = 42,
                        FontWeight = FontWeights.SemiBold,
                        FontFamily = new FontFamily("Segoe UI Variable Display"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Color.FromArgb(143, 221, 241, 255))
                    },
                    new TextBlock
                    {
                        Text = "Thanks for your support during the Fairmark ALPHA!\nExplore a world of simple, clean notes. Enjoy!",
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 12, 0, 0)
                    },
                }
            };
            welcomeDialog.SecondaryButtonClick += (s, e) =>
            {
                Application.Current.Exit();
            };
            await welcomeDialog.ShowAsync();
        }

        private void DebugLoaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            (sender as Grid).Visibility = Visibility.Visible;
#endif
        }

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            if (SideGrid.RowDefinitions[1].Height == new GridLength(1, GridUnitType.Star) && (MainGrid.ColumnDefinitions[0].Width.Value == 300)) {
                ClosePane_Click(null, null);
            }
            else
            {
                Explorer.IsChecked = true;
                Search.IsChecked = false;
            }
            SideGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            SideGrid.RowDefinitions[2].Height = new GridLength(0);
            if (MainGrid.ColumnDefinitions[0].Width.Value == 0)
            {
                var helper = new GridLengthAnimationHelper
                {
                    TargetColumn = MainGrid.ColumnDefinitions[0],
                    AnimatedValue = 0
                };

                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = 300,
                    Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                    EnableDependentAnimation = true,
                    EasingFunction = new CubicEase
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };

                Storyboard.SetTarget(animation, helper);
                Storyboard.SetTargetProperty(animation, "AnimatedValue");

                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (SideGrid.RowDefinitions[2].Height == new GridLength(1, GridUnitType.Star) && (MainGrid.ColumnDefinitions[0].Width.Value == 300)) {
                ClosePane_Click(null, null);
            }
            else
            {
                Explorer.IsChecked = false;
                Search.IsChecked = true;
            }
            SideGrid.RowDefinitions[1].Height = new GridLength(0);
            SideGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
            if (MainGrid.ColumnDefinitions[0].Width.Value == 0)
            {
                var helper = new GridLengthAnimationHelper
                {
                    TargetColumn = MainGrid.ColumnDefinitions[0],
                    AnimatedValue = 0
                };

                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = 300,
                    Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                    EnableDependentAnimation = true,
                    EasingFunction = new CubicEase
                    {
                        EasingMode = EasingMode.EaseOut
                    }
                };

                Storyboard.SetTarget(animation, helper);
                Storyboard.SetTargetProperty(animation, "AnimatedValue");

                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
        }

        private void ClosePane_Click(object sender, RoutedEventArgs e)
        {
            Explorer.IsChecked = false;
            Search.IsChecked = false;
            var helper = new GridLengthAnimationHelper
            {
                TargetColumn = MainGrid.ColumnDefinitions[0],
                AnimatedValue = 300
            };

            var animation = new DoubleAnimation
            {
                From = 300,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                EnableDependentAnimation = true
            };

            Storyboard.SetTarget(animation, helper);
            Storyboard.SetTargetProperty(animation, "AnimatedValue");

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TabView_TabCloseRequested(Microsoft.UI.Xaml.Controls.TabView sender, Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs args)
        {

        }

        private void contentFrame_Loaded(object sender, RoutedEventArgs e)
        {
            Explorer_Click(null, null);
            if (contentFrame.Content == null || (contentFrame.Content != null && contentFrame.Content.GetType() != typeof(EmptyTabPage))) contentFrame.Navigate(typeof(EmptyTabPage));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Variables.firstStartup)
            {
                await WelcomeDialog();
            }
            await NoteCollectionHelper.Initialize();
            ApplicationData.Current.LocalSettings.Values["firstStartup"] = false;
            if (NoteCollectionHelper.notes.Count == 0)
            {
                NoNoteText.Visibility = Visibility.Visible;
            }
            else
            {
                NoNoteText.Visibility = Visibility.Collapsed;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Flyout mainFlyout = new Flyout();
            TextBox box = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxWidth = 300,
                BorderThickness = new Thickness(0),
                PlaceholderText = "Name your note",
                Padding = new Thickness(9, 9, 4, 4),
                FontSize = 13,
                CornerRadius = new CornerRadius(4),
                Height = 40,
                IsReadOnly = false
            };
            Grid.SetColumn(box, 1);

            Button emojiButton = new Button()
            {
                Content = "📋",
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Width = 40,
                Height = 40,
                FontSize = 20
            };
            Grid.SetColumn(emojiButton, 0);

            Grid grid = new Grid
            {
                Width = 200,
                Height = 40,
                Margin = new Thickness(-10),
                ColumnDefinitions =
        {
            new ColumnDefinition { Width = new GridLength(45) },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        }
            };
            grid.Children.Add(emojiButton);
            grid.Children.Add(box);
            mainFlyout.Content = grid;
               
            // Emoji flyout setup
            Flyout flyout = new Flyout();
            StackPanel emojiPanel = new StackPanel() { Orientation = Orientation.Vertical };
            var gridView = new GridView();
            gridView.ItemsPanel = (ItemsPanelTemplate)Application.Current.Resources["WrapGridPanel"];
            gridView.ItemTemplate = (DataTemplate)Application.Current.Resources["EmojiBlock"];
            gridView.ItemsSource = new EmojiHelper.IncrementalEmojiSource();
            gridView.SelectionMode = ListViewSelectionMode.Single;
            gridView.SelectionChanged += (s, args) =>
            {
                emojiButton.Content = gridView.SelectedItem;
                flyout.Hide();
            };
            AutoSuggestBox searchBox = new AutoSuggestBox() { PlaceholderText = "Search for an emoji...", Margin = new Thickness(0, 0, 0, 10), Width = 240, MaxWidth = 240, QueryIcon = new SymbolIcon(Symbol.Find) };
            searchBox.TextChanged += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    gridView.ItemsSource = new EmojiHelper.IncrementalEmojiSource();
                }
                else
                {
                    var searchTerm = searchBox.Text.ToLower();
                    gridView.ItemsSource = EmojiHelper.Emojis.Where(emoji => emoji.SearchTerms.Any(term => term.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }
            };
            emojiPanel.Children.Add(searchBox);
            emojiPanel.Children.Add(gridView);
            flyout.Content = emojiPanel;
            emojiButton.Flyout = flyout;

            box.KeyDown += async (s, args) =>
            {
                if (args.Key == Windows.System.VirtualKey.Enter)
                {
                    if (!string.IsNullOrWhiteSpace(box.Text))
                    {
                        NoNoteText.Visibility = Visibility.Collapsed;
                        string newId;
                        do
                        {
                            newId = Guid.NewGuid().ToString();
                        }
                        while (NoteCollectionHelper.notes.Any(n => n.Id == newId));

                        NoteMetadata meta = new NoteMetadata
                        {
                            Name = box.Text,
                            Emoji = emojiButton.Content.ToString(),
                            Id = newId,
                            Tags = new ObservableCollection<NoteTag>()
                        };
                        if (currentTag != null) meta.Tags.Add(currentTag);
                        NoteCollectionHelper.notes.Add(meta);
                        await NoteCollectionHelper.SaveNotes();
                        NoteList.SelectedItem = meta;
                        contentFrame.Navigate(typeof(FileEditorPage), meta);
                        mainFlyout.Hide();
                    }
                    else
                    {
                        box.PlaceholderText = "Enter a name.";
                    }
                }
            };

            if (sender is FrameworkElement fe)
                mainFlyout.ShowAt(fe);
            else
                mainFlyout.ShowAt(this);
        }


        private async void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteList.SelectedItem != null)
            {
                NoteMetadata selectedNote = NoteList.SelectedItem as NoteMetadata;
                ContentDialog renameDialog = new ContentDialog
                {
                    Title = "Rename note THIS WILL BE A FLYOUT",
                    PrimaryButtonText = "Rename",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary
                };
                TextBox renameBox = new TextBox
                {
                    Text = selectedNote.Name,
                    PlaceholderText = "Enter new name",
                    Margin = new Thickness(0, 0, 0, 10)
                };
                renameDialog.Content = renameBox;
                renameDialog.PrimaryButtonClick += async (s, args) =>
                {
                    if (!string.IsNullOrWhiteSpace(renameBox.Text))
                    {
                        selectedNote.Name = renameBox.Text;
                        await NoteCollectionHelper.SaveNotes();
                    }
                };
                await renameDialog.ShowAsync();
            };
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteList.SelectedItem != null)
            {
                NoteMetadata selectedNote = NoteList.SelectedItem as NoteMetadata;
                ContentDialog deleteDialog = new ContentDialog
                {
                    Title = "Delete note",
                    Content = $"Are you sure you want to delete '{selectedNote.Name}'?",
                    PrimaryButtonText = "Delete",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Secondary
                };
                deleteDialog.PrimaryButtonClick += async (s, args) =>
                {
                    NoteCollectionHelper.notes.Remove(selectedNote);
                    await NoteCollectionHelper.SaveNotes();
                    if (contentFrame.Content == null || (contentFrame.Content != null && contentFrame.Content.GetType() != typeof(EmptyTabPage))) contentFrame.Navigate(typeof(EmptyTabPage));
                    if (NoteCollectionHelper.notes.Count == 0)
                    {
                        NoNoteText.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        NoNoteText.Visibility = Visibility.Collapsed;
                    }
                };
                await deleteDialog.ShowAsync();
            }
        }

        private void TagBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                NoNoteText.Visibility = Visibility.Collapsed;
                var query = sender.Text?.ToLower() ?? string.Empty;
                var filtered = NoteCollectionHelper.tags
                    .Where(tag => tag.Name != null && tag.Name.ToLower().Contains(query))
                    .ToList();

                sender.ItemsSource = filtered;
            }
            else
            {
                sender.ItemsSource = NoteCollectionHelper.tags;
            }
            if (sender.Text == string.Empty)
            {
                NoteList.ItemsSource = NoteCollectionHelper.notes;
                currentTag = null;
                if (NoteCollectionHelper.notes.Count == 0)
{
    NoNoteText.Visibility = Visibility.Visible;
}
else
{
    NoNoteText.Visibility = Visibility.Collapsed;
}
            }
        }

        private async void CreateTag_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog mainflyout = new ContentDialog();
            mainflyout.Title = "Create a new tag (THIS WILL BY A FLYOUT I JUST CANT TYPE INTO TEXTBOXES IN FLYOUTS FOR SOME REASON)";
            TextBox box = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxWidth = 300,
                BorderThickness = new Thickness(0),
                PlaceholderText = "Name your tag..",
                Padding = new Thickness(9, 9, 4, 4),
                FontSize = 13,
                CornerRadius = new CornerRadius(4),
                Height = 40,
                IsReadOnly = false
            };
            Grid.SetColumn(box, 2);

            Button emojiButton = new Button()
            {
                Content = "📋",
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                FontSize = 20,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetColumn(emojiButton, 1);

            // Emoji
            Flyout flyout = new Flyout();
            StackPanel emojiPanel = new StackPanel() { Orientation = Orientation.Vertical };
            var gridView = new GridView();
            gridView.ItemsPanel = (ItemsPanelTemplate)Application.Current.Resources["WrapGridPanel"];
            gridView.ItemTemplate = (DataTemplate)Application.Current.Resources["EmojiBlock"];
            gridView.ItemsSource = new EmojiHelper.IncrementalEmojiSource();
            gridView.SelectionMode = ListViewSelectionMode.Single;
            gridView.SelectionChanged += (s, args) =>
            {
                emojiButton.Content = gridView.SelectedItem;
                flyout.Hide();
            };
            AutoSuggestBox searchBox = new AutoSuggestBox() { PlaceholderText = "Search for an emoji...", Margin = new Thickness(0, 0, 0, 10), Width = 240, MaxWidth = 240, QueryIcon = new SymbolIcon(Symbol.Find) };
            searchBox.TextChanged += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    gridView.ItemsSource = new EmojiHelper.IncrementalEmojiSource();
                }
                else
                {
                    var searchTerm = searchBox.Text.ToLower();
                    gridView.ItemsSource = EmojiHelper.Emojis.Where(emoji => emoji.SearchTerms.Any(term => term.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }
            };
            emojiPanel.Children.Add(searchBox);
            emojiPanel.Children.Add(gridView);
            flyout.Content = emojiPanel;
            emojiButton.Flyout = flyout;
            // end Emoji

            Button tagButton = new Button()
            {
                Background = new SolidColorBrush(Colors.LightBlue),
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 40,
                FontSize = 20
            };
            Grid.SetColumn(tagButton, 0);

            Grid grid = new Grid
            {
                Width = 200,
                Height = 40,
                Margin = new Thickness(-10),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(45) },
                    new ColumnDefinition { Width = new GridLength(55) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };
            grid.Children.Add(tagButton);
            grid.Children.Add(emojiButton);
            grid.Children.Add(box);
            mainflyout.Content = grid;

            Flyout pickerflyout = new Flyout();
            ColorPicker picker = new ColorPicker
            {
                ColorSpectrumShape = ColorSpectrumShape.Ring,
                Color = Colors.LightBlue
            };
            picker.ColorChanged += (s, args) =>
            {
                tagButton.Background = new SolidColorBrush(picker.Color);
            };
            pickerflyout.Content = picker;
            tagButton.Flyout = pickerflyout;

            box.KeyDown += async (s, args) =>
            {
                if (args.Key == Windows.System.VirtualKey.Enter)
                {
                    if (!string.IsNullOrWhiteSpace(box.Text))
                    {
                        NoteCollectionHelper.tags.Add(new NoteTag
                        {
                            Name = box.Text,
                            Emoji = emojiButton.Content.ToString(),
                            Color = picker.Color
                        });
                        await NoteCollectionHelper.SaveTags();
                        mainflyout.Hide();
                    }
                    else
                    {
                        box.PlaceholderText = "Enter a name.";
                    }
                }
            };

            await mainflyout.ShowAsync();
        }

        private void TagBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is NoteTag selectedTag)
            {
                sender.Text = selectedTag.Name;
                currentTag = selectedTag;
                ObservableCollection<NoteMetadata> filteredNotes = new ObservableCollection<NoteMetadata>();
                NoteList.ItemsSource = filteredNotes;
                foreach (NoteMetadata note in NoteCollectionHelper.notes)
                {
                    if (note.Tags != null && note.Tags.Any(t => t.Name == selectedTag.Name && t.Color == selectedTag.Color && t.Emoji == selectedTag.Emoji))
                    {
                        filteredNotes.Add(note); 
                        break;
                    }
                }
            }
        }

        private void MenuFlyoutSubItem_Loaded(object sender, RoutedEventArgs e)
        {
            MenuFlyoutSubItem item = sender as MenuFlyoutSubItem;
            item.Items.Clear();
            foreach (NoteTag tag in NoteCollectionHelper.tags)
            {
                MenuFlyoutItem mfi = new MenuFlyoutItem()
                {
                    Text = tag.Emoji + " " + tag.Name,
                    Background = (new ColorGradientConverter()).Convert(tag.Color, null, null, null) as LinearGradientBrush,
                };
                mfi.Click += async (s, a) =>
                {
                    if (NoteList.SelectedItem != null && NoteList.SelectedItem is NoteMetadata note)
                    {
                        if (!note.Tags.Any(t => t.Name == tag.Name && t.Color == tag.Color && t.Emoji == tag.Emoji))
                        {
                            note.Tags.Add(tag);
                            await NoteCollectionHelper.SaveNotes();
                        }
                        else
                        {
                            var toRemove = note.Tags.FirstOrDefault(t => t.Name == tag.Name && t.Color == tag.Color && t.Emoji == tag.Emoji);
                            if (toRemove != null)
                                note.Tags.Remove(toRemove);
                            await NoteCollectionHelper.SaveNotes();
                        };
                    }
                };
                item.Items.Add(mfi);
            }
        }
    }
}

using CommunityToolkit.WinUI.Media;
using Fairmark.Converters;
using Fairmark.Helpers;
using Fairmark.Models;
using Microsoft.UI.Xaml.Controls;
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
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using ColorPicker = Microsoft.UI.Xaml.Controls.ColorPicker;
using ColorSpectrumShape = Microsoft.UI.Xaml.Controls.ColorSpectrumShape;

namespace Fairmark
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.SetTitleBar(DragRegion);
            Settings.AddHandler(UIElement.PointerPressedEvent,
                new PointerEventHandler(AppBarButton_PointerPressed), true);
            Settings.AddHandler(UIElement.PointerReleasedEvent,
                new PointerEventHandler(AppBarButton_PointerReleased), true);
            Helpers.Settings s = new Settings();
            if (s.HideFromRecall) {
                ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = false;
            }
            else {
                ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = true;
            }
            if (s.AuthenticationEnabled) {
                if (UserConsentVerifier.CheckAvailabilityAsync().AsTask().Result == UserConsentVerifierAvailability.Available) {
                    UserConsentVerifier.RequestVerificationAsync("Fairmark needs your permission to continue. You can turn this off in Settings.").AsTask();
                }
                else {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "Authentication was turned off due to a problem with Windows Hello.";
                    dialog.Content = "Make sure you didn't turn off your PIN/password/biometrics in Windows Settings.";
                    s.AuthenticationEnabled = false;
                    dialog.PrimaryButtonText = "OK";
                    dialog.ShowAsync().AsTask();
                }
            }

        }

        private NoteTag currentTag = null;
        private List<Microsoft.UI.Xaml.Controls.TabViewItem> openTabs = new List<Microsoft.UI.Xaml.Controls.TabViewItem>();

        private async Task WelcomeDialog() {
            var overlay = new Windows.UI.Xaml.Shapes.Rectangle {
                Fill = Application.Current.Resources["OverlayBrush"] as Brush,
                Opacity = 1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            ContentGrid.Children.Add(overlay);
            Grid.SetRowSpan(overlay, ContentGrid.RowDefinitions.Count > 0 ? ContentGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(overlay, ContentGrid.ColumnDefinitions.Count > 0 ? ContentGrid.ColumnDefinitions.Count : 1);

            ContentDialog dialog = new OOBEFrameContentDialog();
            dialog.Closed += async (s, a) => {
                var storyboard = new Storyboard();
                var fadeOut = new DoubleAnimation {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    EnableDependentAnimation = true
                };
                Storyboard.SetTarget(fadeOut, overlay);
                Storyboard.SetTargetProperty(fadeOut, "Opacity");
                storyboard.Children.Add(fadeOut);

                var tcs = new TaskCompletionSource<bool>();
                storyboard.Completed += (snd, evt) => tcs.SetResult(true);
                storyboard.Begin();
                await tcs.Task;
                ContentGrid.Children.Remove(overlay);
            };
            await dialog.ShowAsync();
        }

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            if (SideGrid.RowDefinitions[1].Height == new GridLength(1, GridUnitType.Star) && (MainGrid.ColumnDefinitions[0].Width.Value == 255)) {
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
                    To = 255,
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
            if (SideGrid.RowDefinitions[2].Height == new GridLength(1, GridUnitType.Star) && (MainGrid.ColumnDefinitions[0].Width.Value == 255)) {
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
                    To = 255,
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
                SearchBox.Text = string.Empty;
                SearchResults.ItemsSource = null;
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
                AnimatedValue = 250
            };

            var animation = new DoubleAnimation
            {
                From = 250,
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
            var tab = TabView.SelectedItem as Microsoft.UI.Xaml.Controls.TabViewItem;
            if (tab != null && tab.Tag is NoteMetadata data)
            {
                NoteList.SelectedItem = data;
                contentFrame.Navigate(typeof(FileEditorPage), data);
            }
            else
            {
                NoteList.SelectedItem = null;
                contentFrame.Navigate(typeof(EmptyTabPage));
            }
        }

        private void TabView_TabCloseRequested(Microsoft.UI.Xaml.Controls.TabView sender, Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs args)
        {
            var closingTab = args.Tab as Microsoft.UI.Xaml.Controls.TabViewItem;
            if (closingTab != null)
            {
                openTabs.Remove(closingTab);
                try {
                    TabView.TabItems.Remove(closingTab);
                }
                catch { }
                if (TabView.TabItems.Count > 0)
                {
                    try { 
                        TabView.SelectedItem = TabView.TabItems[TabView.TabItems.Count - 1];
                    }
                    catch { }
                }
                else
                {
                    NoteList.SelectedItem = null;
                    contentFrame.Navigate(typeof(EmptyTabPage));
                }
            }
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


        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteList.SelectedItem != null)
            {
                NoteMetadata selectedNote = NoteList.SelectedItem as NoteMetadata;
                Flyout renameFlyout = new Flyout();
                TextBox renameBox = new TextBox
                {
                    Text = selectedNote.Name,
                    Height = 32,
                    Width = 200,
                    FontSize = 13,
                    VerticalContentAlignment = VerticalAlignment.Bottom,
                    PlaceholderText = "Enter new name",
                    Margin = new Thickness(-10)
                };
                renameFlyout.Content = renameBox;
                renameBox.KeyDown += async (s, args) =>
                {
                    if (args.Key == Windows.System.VirtualKey.Enter)
                    {
                        if (!string.IsNullOrWhiteSpace(renameBox.Text))
                        {
                            selectedNote.Name = renameBox.Text;
                            await NoteCollectionHelper.SaveNotes();
                            renameFlyout.Hide();
                        }
                        else
                        {
                            renameBox.PlaceholderText = "Enter a name.";
                        }
                    }
                };
                renameFlyout.ShowAt(sender as FrameworkElement);
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
                    if (TabView.TabItems.Count > 0)
                    {
                        var tab = TabView.TabItems.FirstOrDefault(t => t is Microsoft.UI.Xaml.Controls.TabViewItem item && item.Tag is NoteMetadata n && n.Id == selectedNote.Id);
                        if (tab != null)
                        {
                            TabView.TabItems.Remove(tab);
                            openTabs.Remove(tab as Microsoft.UI.Xaml.Controls.TabViewItem);
                        }
                    }
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

        private void CreateTag_Click(object sender, RoutedEventArgs e)
        {
            Flyout mainflyout = new Flyout();
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
                Margin = new Thickness(5, 0, 5, 0)
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
                Width = 300,
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

            mainflyout.ShowAt(sender as FrameworkElement);
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

        private void MenuFlyoutItem_Loaded(object sender, RoutedEventArgs e)
        {
            Flyout flyout = new Flyout();
            StackPanel emojiPanel = new StackPanel() { Orientation = Orientation.Vertical };
            var gridView = new GridView();
            gridView.ItemsPanel = (ItemsPanelTemplate)Application.Current.Resources["WrapGridPanel"];
            gridView.ItemTemplate = (DataTemplate)Application.Current.Resources["EmojiBlock"];
            gridView.ItemsSource = new EmojiHelper.IncrementalEmojiSource();
            gridView.SelectionMode = ListViewSelectionMode.Single;
            gridView.SelectionChanged += async (s, args) =>
            {
                NoteCollectionHelper.notes.Where(t => t.Id == ((NoteMetadata)NoteList.SelectedItem).Id).FirstOrDefault().Emoji = gridView.SelectedItem.ToString();
                await NoteCollectionHelper.SaveNotes();
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
            (sender as MenuFlyoutItem).Click += (s, a) =>
            {
                flyout.ShowAt(MoreBtn);
            };
        }

        private void NoteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NoteList.SelectedItem != null)
            {
                NoteMetadata selectedNote = NoteList.SelectedItem as NoteMetadata;
                var tab = openTabs.FirstOrDefault(t => t.Tag is NoteMetadata n && n.Id == selectedNote.Id);
                if (tab == null)
                {
                    // Create header with bindings
                    var header = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                    };
                    var fontIcon = new FontIcon
                    {
                        FontFamily = new FontFamily("Segoe UI Emoji"),
                        Margin = new Thickness(0, 0, 6, 0),
                        FontSize = 13
                    };
                    var emojiBinding = new Windows.UI.Xaml.Data.Binding
                    {
                        Path = new PropertyPath("Emoji"),
                        Source = selectedNote,
                        Mode = Windows.UI.Xaml.Data.BindingMode.OneWay
                    };
                    fontIcon.SetBinding(FontIcon.GlyphProperty, emojiBinding);

                    var textBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    var nameBinding = new Windows.UI.Xaml.Data.Binding
                    {
                        Path = new PropertyPath("Name"),
                        Source = selectedNote,
                        Mode = Windows.UI.Xaml.Data.BindingMode.OneWay
                    };
                    textBlock.SetBinding(TextBlock.TextProperty, nameBinding);

                    header.Children.Add(fontIcon);
                    header.Children.Add(textBlock);

                    tab = new Microsoft.UI.Xaml.Controls.TabViewItem
                    {
                        Header = header,
                        Tag = selectedNote,
                    };
                    openTabs.Add(tab);
                    TabView.TabItems.Add(tab);
                }
                TabView.SelectedItem = tab;
                contentFrame.Navigate(typeof(FileEditorPage), selectedNote);
            }
            else
            {
                contentFrame.Navigate(typeof(EmptyTabPage));
                TabView.SelectedItem = null;
            }
        }

        private void TagBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem is NoteTag selectedTag)
            {
                (sender as ComboBox).SelectedItem = selectedTag;
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
            else
            {
                currentTag = null;
                NoteList.ItemsSource = NoteCollectionHelper.notes;
            }
        }

        private void ClearTags_Click(object sender, RoutedEventArgs e)
        {
            currentTag = null;
            TagBox.SelectedItem = null;
            NoteList.ItemsSource = NoteCollectionHelper.notes;
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
            List<NoteMetadata> filteredNotes = new List<NoteMetadata>();
            if (!string.IsNullOrEmpty(args.QueryText)) {
                SearchResults.ItemsSource = NoteCollectionHelper.notes.Where(n => n.Name.Contains(args.QueryText, StringComparison.InvariantCultureIgnoreCase)).ToList();
            } else {
                SearchResults.ItemsSource = null;
            }
        }

        private void SearchResult_Click(object sender, RoutedEventArgs e) {
            ClearTags_Click(null, null);
            NoteList.SelectedItem = (sender as Button).Tag as NoteMetadata;
            contentFrame.Navigate(typeof(FileEditorPage), NoteList.SelectedItem);
            Explorer_Click(null, null);
            SearchBox.Text = string.Empty;
            SearchResults.ItemsSource = null;
        }

        private void AppBarButton_PointerEntered(object sender, PointerRoutedEventArgs e) {
            Microsoft.UI.Xaml.Controls.AnimatedIcon.SetState((UIElement)sender, "PointerOver");
        }

        private void AppBarButton_PointerPressed(object sender, PointerRoutedEventArgs e) {
            Microsoft.UI.Xaml.Controls.AnimatedIcon.SetState((UIElement)sender, "Pressed");
        }

        private void AppBarButton_PointerReleased(object sender, PointerRoutedEventArgs e) {
            Microsoft.UI.Xaml.Controls.AnimatedIcon.SetState((UIElement)sender, "Normal");
        }

        private void AppBarButton_PointerExited(object sender, PointerRoutedEventArgs e) {
            Microsoft.UI.Xaml.Controls.AnimatedIcon.SetState((UIElement)sender, "Normal");
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e) {
            Settings.KeyboardAccelerators.Add(new KeyboardAccelerator
            {
                Key = (VirtualKey)188,
                Modifiers = VirtualKeyModifiers.Control,
                IsEnabled = true
            });
        }

        private async void Settings_Click(object sender, RoutedEventArgs e) {
            AppWindow window = await AppWindow.TryCreateAsync();
            Frame f = new Frame();
            f.Margin = new Thickness(0, 50, 0, 0);
            f.Navigate(typeof(SettingsPage));
            window.Title = "Settings";
            window.TitleBar.ExtendsContentIntoTitleBar = true;
            window.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            window.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            ElementCompositionPreview.SetAppWindowContent(window, f);
            await window.TryShowAsync();
            Settings.IsEnabled = false;
            window.Closed += (s, a) => {
                Settings.IsEnabled = true;
            };
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode) {
                ClosePane_Click(null, null);
                ContentGrid.RowDefinitions[0].Height = new GridLength(0);
                this.Background = Application.Current.Resources["ZenBG"] as SolidColorBrush;
            }
            else
            {
                ContentGrid.RowDefinitions[0].Height = new GridLength(48);
                this.Background = new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}

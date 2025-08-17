using Fairmark.Converters;
using Fairmark.Helpers;
using Fairmark.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using ColorPicker = Microsoft.UI.Xaml.Controls.ColorPicker;
using ColorSpectrumShape = Microsoft.UI.Xaml.Controls.ColorSpectrumShape;

namespace Fairmark
{
    public sealed partial class MainPage : Page
    {
        public ResourceLoader loader = ResourceLoader.GetForCurrentView();
        public MainPage()
        {
            App.OnUriLaunch += (vault, id) => {
                NoteMetadata data = NoteCollectionHelper.notes.FirstOrDefault(x => x.Id == id);
                if (data != null) {
                    NoteList.SelectedItem = data;
                    NoteList_SelectionChanged(NoteList, null);
                }
            };
            this.InitializeComponent();
            Variables.PlusStatusChanged += async (sender, e) =>
            {
                await PlusCheck();
            };
            Window.Current.SetTitleBar(DragRegion);
            Helpers.Settings s = new Settings();
            if (Window.Current.Content is Frame frame)
            {
                ElementTheme reqtheme = ElementTheme.Default;
                switch (s.Theme)
                {
                    case "Light":
                        reqtheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        reqtheme = ElementTheme.Dark;
                        break;
                    case "Default":
                        reqtheme = ElementTheme.Default;
                        break;
                }
                RequestedTheme = reqtheme;
                (Window.Current.Content as Frame).RequestedTheme = reqtheme;
                ContentGrid.RequestedTheme = reqtheme;
                if (reqtheme == ElementTheme.Default)
                {
                    ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = null;
                    Debug.WriteLine(ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor.HasValue);
                }
                else if (reqtheme == ElementTheme.Dark)
                {
                    ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
                }
                else if (reqtheme == ElementTheme.Light)
                {
                    ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
                }
            }
            (Application.Current.Resources["Settings"] as Settings).ThemeSettingChanged += (sender, e) =>
            {
                RequestedTheme = e.Theme;
                (Window.Current.Content as Frame).RequestedTheme = e.Theme;
                ContentGrid.RequestedTheme = e.Theme;
                if (e.Theme == ElementTheme.Default)
                {
                    ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Red; // TODO its black no matter what
                }
                else if (e.Theme == ElementTheme.Dark)
                {
                    ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
                }
                else if (e.Theme == ElementTheme.Light)
                {
                    ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
                }
            };
            if (s.HideFromRecall)
            {
                ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = false;
            }
            else
            {
                ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = true;
            }
            if (s.AuthenticationEnabled)
            {
                if (UserConsentVerifier.CheckAvailabilityAsync().AsTask().Result == UserConsentVerifierAvailability.Available)
                {
                    _ = UserConsentVerifier.RequestVerificationAsync(loader.GetString("AuthTitle")).AsTask();
                }
                else
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = loader.GetString("AuthOffDialogTitle");
                    dialog.Content = loader.GetString("AuthOffDialogDesc");
                    s.AuthenticationEnabled = false;
                    dialog.PrimaryButtonText = loader.GetString("OK");
                    _ = dialog.ShowAsync().AsTask();
                }
            }

        }

        private List<NoteTag> currentTags = new List<NoteTag>();
        private List<Microsoft.UI.Xaml.Controls.TabViewItem> openTabs = new List<Microsoft.UI.Xaml.Controls.TabViewItem>();

        private async Task WelcomeDialog()
        {
            var overlay = new Windows.UI.Xaml.Shapes.Rectangle
            {
                Fill = Application.Current.Resources["OverlayBrush"] as Brush,
                Opacity = 1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            ContentGrid.Children.Add(overlay);
            Grid.SetRowSpan(overlay, ContentGrid.RowDefinitions.Count > 0 ? ContentGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(overlay, ContentGrid.ColumnDefinitions.Count > 0 ? ContentGrid.ColumnDefinitions.Count : 1);

            ContentDialog dialog = new OOBEFrameContentDialog();
            dialog.Closed += async (s, a) =>
            {
                var storyboard = new Storyboard();
                var fadeOut = new DoubleAnimation
                {
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
                _ = await tcs.Task;
                _ = ContentGrid.Children.Remove(overlay);
                FirstRunTips();
                SecondRunReviewTip();
            };
            _ = await dialog.ShowAsync();
        }

        private async void SecondRunReviewTip() {
            if (Variables.secondStartup && !Variables.firstStartup && Variables.useStoreFeatures)
            {
                await Task.Delay(120000);
                ReviewTip.IsOpen = true;
                ApplicationData.Current.LocalSettings.Values["secondStartup"] = false;
            }
        }

        public TeachingTip addnotetip = new TeachingTip()
        {
            Title = ResourceLoader.GetForCurrentView().GetString("AddNoteTipTitle"),
            Subtitle = ResourceLoader.GetForCurrentView().GetString("AddNoteTipSubtitle"),
            IsOpen = false
        };
        public TeachingTip addtagtip = new TeachingTip()
        {
            Title = ResourceLoader.GetForCurrentView().GetString("AddTagTipTitle"),
            Subtitle = ResourceLoader.GetForCurrentView().GetString("AddTagTipSubtitle"),
            IsOpen = false
        };
        private void FirstRunTips() {
            addnotetip.Target = CreateButton;
            addtagtip.Target = CreateTag;
            MainGrid.Children.Add(addnotetip);
            MainGrid.Children.Add(addtagtip);

            addnotetip.IsOpen = true;
            CreateButton.Click += (s, e) =>
            {
                addnotetip.IsOpen = false;
            };

            NoteList.SelectionChanged += FirstNoteDialog;

            addnotetip.Closed += AddNoteTip_Closed;
            ApplicationData.Current.LocalSettings.Values["firstStartup"] = false;
            ApplicationData.Current.LocalSettings.Values["secondStartup"] = true;
        }

        private void AddNoteTip_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            if (!mainFlyout.IsOpen) { addtagtip.IsOpen = true; }
            else
            {
                mainFlyout.Closed += (s, e) =>
                {
                    addtagtip.IsOpen = true;
                };
            }
            addnotetip.Closed -= AddNoteTip_Closed;
        }

        private void FirstNoteDialog(object sender, SelectionChangedEventArgs e)
        {
            //TeachingTip scrolltip = new TeachingTip()
            //{
            //    Title = "Use the editor to scroll across your note.",
            //    Subtitle = "The preview will scroll automatically.",
            //    PreferredPlacement = TeachingTipPlacementMode.BottomRight,
            //    IsOpen = false
            //};
            //MainGrid.Children.Add(scrolltip);
            //NoteList.SelectionChanged -= FirstNoteDialog;
            //addtagtip.IsOpen = false;
            //scrolltip.IsOpen = true;
        }

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            if (SideGrid.RowDefinitions[1].Height == new GridLength(1, GridUnitType.Star) && (MainGrid.ColumnDefinitions[0].Width.Value == 255))
            {
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
            if (SideGrid.RowDefinitions[2].Height == new GridLength(1, GridUnitType.Star) && (MainGrid.ColumnDefinitions[0].Width.Value == 255))
            {
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
                _ = contentFrame.Navigate(typeof(FileEditorPage), data);
            }
            else
            {
                NoteList.SelectedItem = null;
                _ = contentFrame.Navigate(typeof(EmptyTabPage));
            }
        }

        private void TabView_TabCloseRequested(Microsoft.UI.Xaml.Controls.TabView sender, Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs args)
        {
            var closingTab = args.Tab;
            if (closingTab != null)
            {
                _ = openTabs.Remove(closingTab);
                try
                {
                    _ = TabView.TabItems.Remove(closingTab);
                }
                catch { }
                if (TabView.TabItems.Count > 0)
                {
                    try
                    {
                        TabView.SelectedItem = TabView.TabItems[TabView.TabItems.Count - 1];
                    }
                    catch { }
                }
                else
                {
                    NoteList.SelectedItem = null;
                    _ = contentFrame.Navigate(typeof(EmptyTabPage));
                }
            }
        }

        private void contentFrame_Loaded(object sender, RoutedEventArgs e)
        {
            Explorer_Click(null, null);
            if (contentFrame.Content == null || (contentFrame.Content != null && contentFrame.Content.GetType() != typeof(EmptyTabPage))) _ = contentFrame.Navigate(typeof(EmptyTabPage));
        }

        public SortOptions sortOptions = new SortOptions {
            sortDefault = true,
            sortTag = false,
            sortName = false
        };

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await PlusCheck();
            await App.LogHelper.InitializeAsync();
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
            sortOptions.PropertyChanged += (s, ev) => {
                SortNotes();
            };
            NoteCollectionHelper.notes.CollectionChanged += (s, ev) => {
                SortNotes();
            };
        }

        private void SortNotes() {
            switch (sortOptions) {
                case { sortDefault: true, sortTag: false, sortName: false }:
                    NoteList.ItemsSource = NoteCollectionHelper.notes;
                    break;
                case { sortDefault: false, sortTag: true, sortName: false }:
                    NoteList.ItemsSource = NoteCollectionHelper.notes
                        .OrderBy(n => string.Join(" ", n.Tags.OrderBy(t => t.Name).Select(t => t.Name)))
                        .ThenBy(n => n.Name);
                    break;
                case { sortDefault: false, sortTag: false, sortName: true }:
                    NoteList.ItemsSource = NoteCollectionHelper.notes.OrderBy(n => n.Name);
                    break;
                default:
                    NoteList.ItemsSource = NoteCollectionHelper.notes;
                    break;
            }
        }

        private async Task PlusCheck() {
            if (!(await Variables.CheckIfPlusAsync())) {
                Zen.IsEnabled = false;
                TabView.Opacity = 0;
                TabView.IsEnabled = false;
            }
            else {
                Zen.IsEnabled = true;
                TabView.Opacity = 1;
                TabView.IsEnabled = true;
            }
        }

        public Flyout mainFlyout = new Flyout();
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox box = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxWidth = 300,
                BorderThickness = new Thickness(0),
                PlaceholderText = loader.GetString("AddNotePlaceholder"),
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
            AutoSuggestBox searchBox = new AutoSuggestBox() { PlaceholderText = loader.GetString("EmojiSearchPlaceholder"), Margin = new Thickness(0, 0, 0, 10), Width = 240, MaxWidth = 240, QueryIcon = new SymbolIcon(Symbol.Find) };
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
                        if (currentTags != null) currentTags.ForEach(t => meta.Tags.Add(t));
                        NoteCollectionHelper.notes.Add(meta);
                        await NoteCollectionHelper.SaveNotes();
                        NoteList.SelectedItem = meta;
                        _ = contentFrame.Navigate(typeof(FileEditorPage), meta);
                        mainFlyout.Hide();
                    }
                    else
                    {
                        box.PlaceholderText = loader.GetString("AddNoteEmptyText");
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
                    PlaceholderText = loader.GetString("RenameNotePlaceholder"),
                    Margin = new Thickness(-10)
                };
                renameFlyout.Content = renameBox;
                renameBox.KeyDown += async (s, args) =>
                {
                    if (args.Key == Windows.System.VirtualKey.Enter)
                    {
                        if (!string.IsNullOrWhiteSpace(renameBox.Text))
                        {
                            App.LogHelper.WriteLog($"Renaming note {selectedNote.Id} from '{selectedNote.Name}' to '{renameBox.Text}'");
                            selectedNote.Name = renameBox.Text;
                            await NoteCollectionHelper.SaveNotes();
                            renameFlyout.Hide();
                        }
                        else
                        {
                            renameBox.PlaceholderText = loader.GetString("AddNoteEmptyText");
                        }
                    }
                };
                renameFlyout.ShowAt(sender as FrameworkElement);
            }
            ;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteList.SelectedItem != null)
            {
                NoteMetadata selectedNote = NoteList.SelectedItem as NoteMetadata;
                ContentDialog deleteDialog = new ContentDialog
                {
                    Title = loader.GetString("DeleteNoteDialogTitle"),
                    Content = string.Format(loader.GetString("DeleteNoteDialogDesc"), selectedNote.Name),
                    PrimaryButtonText = loader.GetString("DeleteButton"),
                    SecondaryButtonText = loader.GetString("CancelButton"),
                    DefaultButton = ContentDialogButton.Secondary
                };
                deleteDialog.PrimaryButtonClick += async (s, args) =>
                {
                    App.LogHelper.WriteLog($"Deleting note {selectedNote.Id} with name '{selectedNote.Name}'");
                    _ = NoteCollectionHelper.notes.Remove(selectedNote);
                    await NoteCollectionHelper.SaveNotes();
                    if (contentFrame.Content == null || (contentFrame.Content != null && contentFrame.Content.GetType() != typeof(EmptyTabPage))) _ = contentFrame.Navigate(typeof(EmptyTabPage));
                    if (TabView.TabItems.Count > 0)
                    {
                        var tab = TabView.TabItems.FirstOrDefault(t => t is Microsoft.UI.Xaml.Controls.TabViewItem item && item.Tag is NoteMetadata n && n.Id == selectedNote.Id);
                        if (tab != null)
                        {
                            _ = TabView.TabItems.Remove(tab);
                            _ = openTabs.Remove(tab as Microsoft.UI.Xaml.Controls.TabViewItem);
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
                _ = await deleteDialog.ShowAsync();
            }
        }

        private void CreateTag_Click(object sender, RoutedEventArgs e)
        {
            addtagtip.IsOpen = false;
            Flyout mainflyout = new Flyout();
            TextBox box = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxWidth = 300,
                BorderThickness = new Thickness(0),
                PlaceholderText = loader.GetString("AddTagPlaceholder"),
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
            AutoSuggestBox searchBox = new AutoSuggestBox() { PlaceholderText = loader.GetString("EmojiSearchPlaceholder"), Margin = new Thickness(0, 0, 0, 10), Width = 240, MaxWidth = 240, QueryIcon = new SymbolIcon(Symbol.Find) };
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
                if (NoteCollectionHelper.tags.Count < 5 || await Variables.CheckIfPlusAsync()) {
                    if (args.Key == Windows.System.VirtualKey.Enter) {
                        if (!string.IsNullOrWhiteSpace(box.Text)) {
                            App.LogHelper.WriteLog($"Creating tag '{box.Text}' with emoji '{emojiButton.Content}'");
                            NoteCollectionHelper.tags.Add(new NoteTag {
                                Name = box.Text,
                                Emoji = emojiButton.Content.ToString(),
                                Color = picker.Color,
                                GUID = Guid.NewGuid().ToString()
                            });
                            await NoteCollectionHelper.SaveTags();
                            mainflyout.Hide();
                        }
                        else {
                            box.PlaceholderText = loader.GetString("AddNoteEmptyText");
                        }
                    }
                }
                else {
                    if (args.Key == Windows.System.VirtualKey.Enter) {
                        box.Text = string.Empty;
                    }
                    box.PlaceholderText = loader.GetString("PlusLimit");
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
                    Background = new ColorGradientConverter().Convert(tag.Color, null, null, null) as LinearGradientBrush,
                    Tag = tag.GUID
                };
                mfi.Click += async (s, a) =>
                {
                    if (NoteList.SelectedItem != null && NoteList.SelectedItem is NoteMetadata note)
                    {
                        var selectedTagGuid = (s as MenuFlyoutItem).Tag as string;
                        var globalTag = NoteCollectionHelper.tags.FirstOrDefault(t => t.GUID == selectedTagGuid);
                        if (globalTag == null)
                            return;

                        var noteTag = note.Tags.FirstOrDefault(t => t.GUID == selectedTagGuid);
                        if (noteTag == null && (note.Tags.Count == 0 || await Variables.CheckIfPlusAsync()))
                        {
                            App.LogHelper.WriteLog($"Adding tag '{globalTag.Name}' to note '{note.Name}'");
                            note.Tags.Add(globalTag);
                            await NoteCollectionHelper.SaveNotes();
                        }
                        else
                        {
                            App.LogHelper.WriteLog($"Removing tag '{globalTag.Name}' from note '{note.Name}'");
                            _ = note.Tags.Remove(noteTag);
                            await NoteCollectionHelper.SaveNotes();
                        }
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
            AutoSuggestBox searchBox = new AutoSuggestBox() { PlaceholderText = loader.GetString("EmojiSearchPlaceholder"), Margin = new Thickness(0, 0, 0, 10), Width = 240, MaxWidth = 240, QueryIcon = new SymbolIcon(Symbol.Find) };
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
                _ = contentFrame.Navigate(typeof(FileEditorPage), selectedNote);
            }
            else
            {
                _ = contentFrame.Navigate(typeof(EmptyTabPage));
                TabView.SelectedItem = null;
            }
        }

        private void TagBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var listView = sender as ListView;
            if (listView?.SelectedItems != null && listView.SelectedItems.Count > 0) {
                var selectedTags = listView.SelectedItems.Cast<NoteTag>().ToList();
                Debug.WriteLine($"Selected tags: {string.Join(", ", selectedTags.Select(t => t.Name))}");
                currentTags = selectedTags;
                ObservableCollection<NoteMetadata> filteredNotes = new ObservableCollection<NoteMetadata>();
                NoteList.ItemsSource = filteredNotes;
                foreach (NoteMetadata note in NoteCollectionHelper.notes) {
                    if (note.Tags != null) {
                        if (selectedTags.All(t => note.Tags.Any(y => y.GUID == t.GUID))) {
                            filteredNotes.Add(note);
                        }
                    }
                }
            }
            else {
                Debug.WriteLine("No tags selected");
                currentTags = null;
                NoteList.ItemsSource = NoteCollectionHelper.notes;
            }
        }

        private void ClearTags_Click(object sender, RoutedEventArgs e)
        {
            currentTags = null;
            TagBox.SelectedItem = null;
            NoteList.ItemsSource = NoteCollectionHelper.notes;
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            List<NoteMetadata> filteredNotes = new List<NoteMetadata>();
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                SearchResults.ItemsSource = NoteCollectionHelper.notes.Where(n => n.Name.Contains(args.QueryText, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
            else
            {
                SearchResults.ItemsSource = null;
            }
        }

        private void SearchResult_Click(object sender, RoutedEventArgs e)
        {
            ClearTags_Click(null, null);
            NoteList.SelectedItem = (sender as Button).Tag as NoteMetadata;
            _ = contentFrame.Navigate(typeof(FileEditorPage), NoteList.SelectedItem);
            Explorer_Click(null, null);
            SearchBox.Text = string.Empty;
            SearchResults.ItemsSource = null;
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: KeyboardAccelerators
        }

        private void About_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Contact_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            AppWindow window = await AppWindow.TryCreateAsync();
            Frame f = new Frame();
            f.Margin = new Thickness(0, 50, 0, 0);
            _ = f.Navigate(typeof(AboutPage));
            window.Title = loader.GetString("AboutTitle");
            window.TitleBar.ExtendsContentIntoTitleBar = true;
            window.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            window.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            var settings = Application.Current.Resources["Settings"] as Settings;
            if (settings != null)
            {
                ElementTheme initialTheme = ElementTheme.Default;
                switch (settings.Theme)
                {
                    case "Light":
                        initialTheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        initialTheme = ElementTheme.Dark;
                        break;
                    default:
                        initialTheme = ElementTheme.Default;
                        break;
                }
                f.RequestedTheme = initialTheme;
                if (initialTheme == ElementTheme.Default)
                {
                    window.TitleBar.ButtonForegroundColor = null;
                }
                else if (initialTheme == ElementTheme.Dark)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.White;
                }
                else if (initialTheme == ElementTheme.Light)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.Black;
                }
            }

            ElementCompositionPreview.SetAppWindowContent(window, f);
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, args) =>
            {
                f.RequestedTheme = args.Theme;
                if (args.Theme == ElementTheme.Default)
                {
                    window.TitleBar.ButtonForegroundColor = null;
                }
                else if (args.Theme == ElementTheme.Dark)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.White;
                }
                else if (args.Theme == ElementTheme.Light)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.Black;
                }
            };
            _ = await window.TryShowAsync();
            TopMore.Flyout.Opening += (s, a) =>
            {
                if (window != null)
                {
                    About.IsEnabled = false;
                }
                else
                {
                    About.IsEnabled = true;
                }
            };
            window.Closed += (s, a) =>
            {
                window = null;
                About.IsEnabled = true;
            };
        }

        private async void Contact_Click(object sender, RoutedEventArgs e)
        {
            AppWindow window = await AppWindow.TryCreateAsync();
            Frame f = new Frame();
            f.Margin = new Thickness(0, 50, 0, 0);
            _ = f.Navigate(typeof(ContactPage));
            window.Title = loader.GetString("ContactTitle");
            window.TitleBar.ExtendsContentIntoTitleBar = true;
            window.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            window.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            var settings = Application.Current.Resources["Settings"] as Settings;
            if (settings != null)
            {
                ElementTheme initialTheme = ElementTheme.Default;
                switch (settings.Theme)
                {
                    case "Light":
                        initialTheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        initialTheme = ElementTheme.Dark;
                        break;
                    default:
                        initialTheme = ElementTheme.Default;
                        break;
                }
                f.RequestedTheme = initialTheme;
                if (initialTheme == ElementTheme.Default)
                {
                    window.TitleBar.ButtonForegroundColor = null;
                }
                else if (initialTheme == ElementTheme.Dark)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.White;
                }
                else if (initialTheme == ElementTheme.Light)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.Black;
                }
            }

            ElementCompositionPreview.SetAppWindowContent(window, f);
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, args) =>
            {
                f.RequestedTheme = args.Theme;
                if (args.Theme == ElementTheme.Default)
                {
                    window.TitleBar.ButtonForegroundColor = null;
                }
                else if (args.Theme == ElementTheme.Dark)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.White;
                }
                else if (args.Theme == ElementTheme.Light)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.Black;
                }
            };
            _ = await window.TryShowAsync();
            TopMore.Flyout.Opening += (s, a) =>
            {
                if (window != null)
                {
                    Contact.IsEnabled = false;
                }
                else
                {
                    Contact.IsEnabled = true;
                }
            };
            window.Closed += (s, a) =>
            {
                window = null;
                Contact.IsEnabled = true;
            };
        }

        private async void Settings_Click(object sender, RoutedEventArgs e)
        {
            AppWindow window = await AppWindow.TryCreateAsync();
            Frame f = new Frame();
            f.Margin = new Thickness(0, 50, 0, 0);
            if (e == null) {
                _ = f.Navigate(typeof(SettingsPage), "tag");
            }
            else {
                _ = f.Navigate(typeof(SettingsPage));
            }
            window.Title = loader.GetString("Settings/Text");
            window.TitleBar.ExtendsContentIntoTitleBar = true;
            window.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            window.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            var settings = Application.Current.Resources["Settings"] as Settings;
            if (settings != null)
            {
                ElementTheme initialTheme = ElementTheme.Default;
                switch (settings.Theme)
                {
                    case "Light": initialTheme = ElementTheme.Light; break;
                    case "Dark": initialTheme = ElementTheme.Dark; break;
                    default: initialTheme = ElementTheme.Default; break;
                }
                f.RequestedTheme = initialTheme;
                if (initialTheme == ElementTheme.Default)
                {
                    window.TitleBar.ButtonForegroundColor = null;
                }
                else if (initialTheme == ElementTheme.Dark)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.White;
                }
                else if (initialTheme == ElementTheme.Light)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.Black;
                }
            }

            ElementCompositionPreview.SetAppWindowContent(window, f);
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, args) =>
            {
                f.RequestedTheme = args.Theme;
                if (args.Theme == ElementTheme.Default)
                {
                    window.TitleBar.ButtonForegroundColor = null;
                }
                else if (args.Theme == ElementTheme.Dark)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.White;
                }
                else if (args.Theme == ElementTheme.Light)
                {
                    window.TitleBar.ButtonForegroundColor = Colors.Black;
                }
            };

            _ = await window.TryShowAsync();
            TopMore.Flyout.Opening += (s, a) =>
            {
                Settings.IsEnabled = window != null ? false : true;
            };
            window.Closed += (s, a) =>
            {
                window = null;
                Settings.IsEnabled = true;
            };
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                ClosePane_Click(null, null);
                AppTitleBar.ColumnDefinitions[0].Width = new GridLength(0);
                MainCommands.Margin = new Thickness(-5, 0, -10, 0);
                this.Background = Application.Current.Resources["ZenBG"] as SolidColorBrush;
            }
            else
            {
                MainCommands.Margin = new Thickness(5, 0, -10, 0);
                AppTitleBar.ColumnDefinitions[0].Width = GridLength.Auto;
                this.Background = new SolidColorBrush(Colors.Transparent);

                if (e.NewSize.Width < 600)
                {
                    if (MainGrid.ColumnDefinitions[0].Width.Value != 0) { ClosePane_Click(null, null); }
                    ;
                    TabView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TabView.Visibility = Visibility.Visible;
                }
            }
        }

        private void TagBox_TextChanged(ComboBox sender, SelectionChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(sender.Text))
            {

                TagBox.ItemsSource = NoteCollectionHelper.tags
                    .Where(t => t.Name.Contains(sender.Text, StringComparison.InvariantCultureIgnoreCase))
                    .Where(t => !((sender.SelectedItem as NoteTag).GUID == t.GUID));
            }
            else
            {
                TagBox.ItemsSource = NoteCollectionHelper.tags
                    .Where(t => !((sender.SelectedItem as NoteTag).GUID == t.GUID));
            }
        }

        private void TagBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = string.Empty;
            TagBox.ItemsSource = NoteCollectionHelper.tags;
        }

        public ApplicationView currentView => ApplicationView.GetForCurrentView();

        private void Zen_Click(object sender, RoutedEventArgs e)
        {
            if (currentView.IsFullScreenMode)
                currentView.ExitFullScreenMode();
            else
                _ = currentView.TryEnterFullScreenMode();
        }

        private void MoreBtn_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Filter_Click(object sender, RoutedEventArgs e) {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void ManageTags_Click(object sender, RoutedEventArgs e) {
            Settings_Click(Settings, null);
        }
    }
}

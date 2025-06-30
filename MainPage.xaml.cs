using Fairmark.Helpers;
using System;
using System.Threading.Tasks;
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
            contentFrame.Navigate(typeof(EmptyTabPage));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Variables.firstStartup)
            {
                await WelcomeDialog();
            }
            ApplicationData.Current.LocalSettings.Values["firstStartup"] = false;
        }
    }
}

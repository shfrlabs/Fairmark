using Fairmark.Helpers;
using System;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
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
            contentFrame.Navigate(typeof(EmptyTabPage));
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            if (!Variables.isVaultSelected)
            {
                this.IsEnabled = false;
                AppWindow window = await AppWindow.TryCreateAsync();
                var titleBar = window.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.BackgroundColor = Colors.Transparent;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverBackgroundColor = Colors.Transparent;
                titleBar.ButtonPressedBackgroundColor = Colors.Transparent;
                titleBar.InactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                window.Title = "Vault selection";
                ElementCompositionPreview.SetAppWindowContent(window, new OOBEPage());
                await window.TryShowAsync();
                window.Closed += (s, e) =>
                {
                    if (Variables.isVaultSelected)
                    {
                        this.IsEnabled = true;
                        contentFrame.Navigate(typeof(EmptyTabPage));
                    }
                    else
                    {
                        Application.Current.Exit();
                    }
                };
            }
        }
    }
}

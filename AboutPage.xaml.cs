using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Fairmark.Helpers;

namespace Fairmark {
    public sealed partial class AboutPage : Page {
        public AboutPage() {
            this.InitializeComponent();
            // Add theme event handler for About window
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, e) =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    frame.RequestedTheme = e.Theme;
                }
                // Update titlebar foreground
                var view = ApplicationView.GetForCurrentView();
                if (e.Theme == ElementTheme.Dark)
                {
                    view.TitleBar.ForegroundColor = Colors.White;
                    view.TitleBar.ButtonForegroundColor = Colors.White;
                }
                else
                {
                    view.TitleBar.ForegroundColor = Colors.Black;
                    view.TitleBar.ButtonForegroundColor = Colors.Black;
                }
            };
        }

        public string Version {
            get {
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }
    }
}

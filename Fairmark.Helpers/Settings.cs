using System;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Fairmark.Helpers {
    public class Settings : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ThemeSetEventHandler ThemeSettingChanged;
        public delegate void ThemeSetEventHandler(object sender, ThemeSetEventArgs e);
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public class ThemeSetEventArgs {
            public ElementTheme Theme { get; set; }
            public ThemeSetEventArgs(ElementTheme theme) {
                Theme = theme;
            }
        }

        public string[] AvailableThemes { get; } =
            new[] { "Default", "Light", "Dark" };

        public string Theme {
            get {
                string current;
                if (_localSettings.Values.TryGetValue("theme", out object themeObj)
                    && themeObj is string themeValue) {
                    current = themeValue;
                }
                else {
                    current = "Default";
                }

                // App.LogHelper.WriteLog($"[Settings] Get Theme -> {current}");
                return current;
            }
            set {
                var old = Theme;
                if (old != value) {
                    // App.LogHelper.WriteLog($"[Settings] Set Theme: {value} (was {old})");
                    _localSettings.Values["theme"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Theme)));
                    ElementTheme newTheme;
                    switch (value) {
                        case "Light":
                            newTheme = ElementTheme.Light;
                            break;
                        case "Dark":
                            newTheme = ElementTheme.Dark;
                            break;
                        default:
                            newTheme = ElementTheme.Default;
                            break;
                    }
                    ThemeSettingChanged?.Invoke(this, new ThemeSetEventArgs(newTheme));
                }
            }
        }

        public FontFamily EditorFontFamily {
            get {
                FontFamily current;
                if (_localSettings.Values.TryGetValue("editorFontFamily", out object fontObj)) {
                    current = new FontFamily(fontObj.ToString());
                }
                else {
                    current = new FontFamily("Consolas");
                }

                // App.LogHelper.WriteLog($"[Settings] Get EditorFontFamily -> {current.Source}");
                return current;
            }
            set {
                var old = EditorFontFamily;
                if (old.Source != value.Source) {
                    // App.LogHelper.WriteLog($"[Settings] Set EditorFontFamily: {value.Source} (was {old.Source})");
                    _localSettings.Values["editorFontFamily"] = value.Source;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditorFontFamily)));
                }
            }
        }

        public FontFamily PreviewFontFamily {
            get {
                FontFamily current;
                if (_localSettings.Values.TryGetValue("previewFontFamily", out object fontObj)) {
                    current = new FontFamily(fontObj.ToString());
                }
                else {
                    current = new FontFamily("Segoe UI Variable Text");
                }

                // App.LogHelper.WriteLog($"[Settings] Get PreviewFontFamily -> {current.Source}");
                return current;
            }
            set {
                var old = PreviewFontFamily;
                if (old.Source != value.Source) {
                    // App.LogHelper.WriteLog($"[Settings] Set PreviewFontFamily: {value.Source} (was {old.Source})");
                    _localSettings.Values["previewFontFamily"] = value.Source;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewFontFamily)));
                }
            }
        }

        public bool HideFromRecall {
            get {
                bool current = _localSettings.Values.TryGetValue("hideFromRecall", out object hideObj)
                               && hideObj is bool b && b;
                // App.LogHelper.WriteLog($"[Settings] Get HideFromRecall -> {current}");
                return current;
            }
            set {
                var old = HideFromRecall;
                if (old != value) {
                    // App.LogHelper.WriteLog($"[Settings] Set HideFromRecall: {value} (was {old})");
                    _localSettings.Values["hideFromRecall"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideFromRecall)));
                }
                ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = !value;
            }
        }

        public bool AutoEmbed {
            get {
                //bool current = _localSettings.Values.TryGetValue("autoEmbed", out object hideObj) && hideObj is bool b && b;
                bool current = false; // disabled temporairly, in case I switch to a renderer that supports HTML
                return current;
            }
            set {
                var old = AutoEmbed;
                if (old != value) {
                    _localSettings.Values["autoEmbed"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoEmbed)));
                }
            }
        }

        public bool AI {
            get {
                bool current = _localSettings.Values.TryGetValue("fairmarkAI", out object hideObj)
                               && hideObj is bool b && b;
                // App.LogHelper.WriteLog($"[Settings] Get AI -> {current}");
                return current;
            }
            set {
                var old = AI;
                if (old != value) {
                    // App.LogHelper.WriteLog($"[Settings] Set AI: {value} (was {old})");
                    _localSettings.Values["fairmarkAI"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AI)));
                }
            }
        }
        public bool AccessLogs {
            get {
                bool current = _localSettings.Values.TryGetValue("accessLogs", out object hideObj)
                               && hideObj is bool b && b;
                // App.LogHelper.WriteLog($"[Settings] Get AccessLogs -> {current}");
                return current;
            }
            set {
                var old = AccessLogs;
                if (old != value) {
                    // App.LogHelper.WriteLog($"[Settings] Set AccessLogs: {value} (was {old})");
                    _localSettings.Values["accessLogs"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccessLogs)));
                }
            }
        }

        public bool AuthenticationEnabled {
            get {
                bool current = _localSettings.Values.TryGetValue("authenticationEnabled", out object authObj)
                               && authObj is bool a && a;
                // App.LogHelper.WriteLog($"[Settings] Get AuthenticationEnabled -> {current}");
                return current;
            }
            set {
                var old = AuthenticationEnabled;
                if (old != value) {
                    // App.LogHelper.WriteLog($"[Settings] Set AuthenticationEnabled: {value} (was {old})");
                    _localSettings.Values["authenticationEnabled"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AuthenticationEnabled)));
                }
            }
        }

        public int EditorFontSize {
            get {
                int current = 14;
                if (_localSettings.Values.TryGetValue("editorFontSize", out object sizeObj)
                    && int.TryParse(sizeObj.ToString(), out int parsed)) {
                    current = parsed;
                }

                // App.LogHelper.WriteLog($"[Settings] Get EditorFontSize -> {current}");
                return current;
            }
            set {
                var old = EditorFontSize;
                if (old != value) {
                    // App.LogHelper.WriteLog($"[Settings] Set EditorFontSize: {value} (was {old})");
                    _localSettings.Values["editorFontSize"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditorFontSize)));
                }
            }
        }

        public int PreviewFontSize {
            get {
                int current = 14;
                if (_localSettings.Values.TryGetValue("previewFontSize", out object sizeObj)
                    && int.TryParse(sizeObj.ToString(), out int parsed)) {
                    current = parsed;
                }

                // App.LogHelper.WriteLog($"[Settings] Get PreviewFontSize -> {current}");
                return current;
            }
            set {
                var old = PreviewFontSize;
                if (old != value) {
                    // App.LogHelper.WriteLog($"[Settings] Set PreviewFontSize: {value} (was {old})");
                    _localSettings.Values["previewFontSize"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PreviewFontSize)));
                }
            }
        }
    }
}
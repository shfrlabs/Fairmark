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
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

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

                Debug.WriteLine($"[Settings] Get Theme -> {current}");
                return current;
            }
            set {
                var old = Theme;
                if (old != value) {
                    Debug.WriteLine($"[Settings] Set Theme: {value} (was {old})");
                    _localSettings.Values["theme"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Theme)));
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

                Debug.WriteLine($"[Settings] Get EditorFontFamily -> {current.Source}");
                return current;
            }
            set {
                var old = EditorFontFamily;
                if (old.Source != value.Source) {
                    Debug.WriteLine($"[Settings] Set EditorFontFamily: {value.Source} (was {old.Source})");
                    _localSettings.Values["editorFontFamily"] = value.Source;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditorFontFamily)));
                }
            }
        }

        public bool HideFromRecall {
            get {
                bool current = _localSettings.Values.TryGetValue("hideFromRecall", out object hideObj)
                               && hideObj is bool b && b;
                Debug.WriteLine($"[Settings] Get HideFromRecall -> {current}");
                return current;
            }
            set {
                var old = HideFromRecall;
                if (old != value) {
                    Debug.WriteLine($"[Settings] Set HideFromRecall: {value} (was {old})");
                    _localSettings.Values["hideFromRecall"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HideFromRecall)));
                }
                ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = !value;
            }
        }

        public bool AI {
            get {
                bool current = _localSettings.Values.TryGetValue("fairmarkAI", out object hideObj)
                               && hideObj is bool b && b;
                Debug.WriteLine($"[Settings] Get AI -> {current}");
                return current;
            }
            set {
                var old = AI;
                if (old != value) {
                    Debug.WriteLine($"[Settings] Set AI: {value} (was {old})");
                    _localSettings.Values["fairmarkAI"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AI)));
                }
            }
        }
        public bool AccessLogs {
            get {
                bool current = _localSettings.Values.TryGetValue("accessLogs", out object hideObj)
                               && hideObj is bool b && b;
                Debug.WriteLine($"[Settings] Get AccessLogs -> {current}");
                return current;
            }
            set {
                var old = AccessLogs;
                if (old != value) {
                    Debug.WriteLine($"[Settings] Set AccessLogs: {value} (was {old})");
                    _localSettings.Values["accessLogs"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccessLogs)));
                }
            }
        }

        public bool AuthenticationEnabled {
            get {
                bool current = _localSettings.Values.TryGetValue("authenticationEnabled", out object authObj)
                               && authObj is bool a && a;
                Debug.WriteLine($"[Settings] Get AuthenticationEnabled -> {current}");
                return current;
            }
            set {
                var old = AuthenticationEnabled;
                if (old != value) {
                    Debug.WriteLine($"[Settings] Set AuthenticationEnabled: {value} (was {old})");
                    _localSettings.Values["authenticationEnabled"] = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AuthenticationEnabled)));
                }
            }
        }
    }
}
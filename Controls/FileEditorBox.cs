using Fairmark.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Fairmark.Controls {
    public sealed class FileEditorBox : Control {
        private TextBox _innerBox;
        private bool _updatingSelection;
        private bool _updatingText;
        private int _lastSelectionStart;
        private int _lastSelectionLength;
        private bool _updatingHeadings;
        private int _instanceId;
        private static int _instanceCounter;

        public FileEditorBox() {
            _instanceId = ++_instanceCounter;
            Debug.WriteLine($"[#{_instanceId}] FileEditorBox created");
            this.DefaultStyleKey = typeof(FileEditorBox);
            this.Loaded += (s, e) => Debug.WriteLine($"[#{_instanceId}] Control loaded");
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FileEditorBox),
            new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        public static readonly DependencyProperty NoteIDProperty =
            DependencyProperty.Register("NoteID", typeof(string), typeof(FileEditorBox),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsSelectionBoldProperty =
            DependencyProperty.Register("IsSelectionBold", typeof(bool), typeof(FileEditorBox),
                new PropertyMetadata(false, OnFormattingPropertyChanged));

        public static readonly DependencyProperty IsSelectionItalicProperty =
            DependencyProperty.Register("IsSelectionItalic", typeof(bool), typeof(FileEditorBox),
                new PropertyMetadata(false, OnFormattingPropertyChanged));

        public static readonly DependencyProperty IsSelectionStrikethroughProperty =
            DependencyProperty.Register("IsSelectionStrikethrough", typeof(bool), typeof(FileEditorBox),
                new PropertyMetadata(false, OnFormattingPropertyChanged));

        public static readonly DependencyProperty IsSelectionCodeProperty =
            DependencyProperty.Register("IsSelectionCode", typeof(bool), typeof(FileEditorBox),
                new PropertyMetadata(false, OnFormattingPropertyChanged));

        public static readonly DependencyProperty IsBulletedProperty =
            DependencyProperty.Register("IsBulleted", typeof(bool), typeof(FileEditorBox),
                new PropertyMetadata(false, OnBulletPropertyChanged));

        public static readonly DependencyProperty IsHeading1Property =
            DependencyProperty.Register("IsHeading1", typeof(bool), typeof(FileEditorBox),
                new PropertyMetadata(false, OnHeadingPropertyChanged));

        public static readonly DependencyProperty IsHeading2Property =
            DependencyProperty.Register("IsHeading2", typeof(bool), typeof(FileEditorBox),
                new PropertyMetadata(false, OnHeadingPropertyChanged));

        public static readonly DependencyProperty IsHeading3Property =
            DependencyProperty.Register("IsHeading3", typeof(bool), typeof(FileEditorBox),
                new PropertyMetadata(false, OnHeadingPropertyChanged));

        public static readonly DependencyProperty HasSelectionProperty =
            DependencyProperty.Register("HasSelection", typeof(bool), typeof(FileEditorBox),
                new PropertyMetadata(false));

        public event EventHandler<CursorLineChangedEventArgs> CursorLineChanged;
        private int _lastCursorLineStart = -1;
        private int _lastCursorLineNumber = 0;
        protected override void OnApplyTemplate() {
            Debug.WriteLine($"[#{_instanceId}] Applying template");
            base.OnApplyTemplate();

            if (_innerBox != null) {
                _innerBox.TextChanged -= InnerBox_TextChanged;
                _innerBox.SelectionChanged -= InnerBox_SelectionChanged;
                _innerBox.PointerPressed -= InnerBox_PointerPressed;
                _innerBox.KeyDown -= InnerBox_KeyDown;
                _innerBox.Paste -= InnerBox_Paste;
            }

            _innerBox = GetTemplateChild("MarkEditor") as TextBox;

            if (_innerBox != null) {
                Debug.WriteLine($"[#{_instanceId}] Inner box found");
                var binding = new Binding {
                    Source = this,
                    Path = new PropertyPath(nameof(Text)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                _innerBox.SetBinding(TextBox.TextProperty, binding);

                _innerBox.TextChanged += InnerBox_TextChanged;
                _innerBox.SelectionChanged += InnerBox_SelectionChanged;
                _innerBox.PointerPressed += InnerBox_PointerPressed;
                _innerBox.KeyDown += InnerBox_KeyDown;
                _innerBox.Paste += InnerBox_Paste;

                _lastSelectionStart = _innerBox.SelectionStart;
                _lastSelectionLength = _innerBox.SelectionLength;
                UpdateSelectionProperties();
            }
            else {
                Debug.WriteLine($"[#{_instanceId}] ERROR: Inner box not found!");
            }
        }

        private void InnerBox_Paste(object sender, TextControlPasteEventArgs e) {
            Debug.WriteLine(">> InnerBox_Paste fired");

            // 1. Early settings check
            var settings = (Application.Current.Resources["Settings"] as Settings);
            Debug.WriteLine($"AutoEmbed setting: {settings?.AutoEmbed}");
            if (settings?.AutoEmbed != true) {
                Debug.WriteLine("AutoEmbed disabled → normal paste");
                return;
            }

            // 2. See if there's text at all
            var dp = Clipboard.GetContent();
            if (!dp.Contains(StandardDataFormats.Text)) {
                Debug.WriteLine("No text on clipboard → normal paste");
                return;
            }

            // 3. At this point we know we want to intercept
            e.Handled = true;
            Debug.WriteLine("Default paste canceled (e.Handled = true)");

            // 4. Fire-and-forget the async embed logic
            if (sender is TextBox tb) {
                _ = EmbedLinkAsync(tb);
            }
            else {
                Debug.WriteLine("Sender isn’t a TextBox – can’t embed");
            }

            Debug.WriteLine("<< InnerBox_Paste exit");
        }

        /// <summary>
        /// Runs async (off the paste handler) to fetch the text, detect
        /// an embed link, and then splice it into the TextBox.
        /// </summary>
        private async Task EmbedLinkAsync(TextBox tb) {
            try {
                // 1. Pull the text
                var dp = Clipboard.GetContent();
                var raw = (await dp.GetTextAsync()).Trim();
                Debug.WriteLine($"Clipboard text: '{raw}'");
                if (string.IsNullOrWhiteSpace(raw)) {
                    Debug.WriteLine("Empty/whitespace → nothing to embed");
                    return;
                }

                // 2. Detect and normalize
                if (!TryGetEmbedLink(raw, out var url)) {
                    Debug.WriteLine("Not a Spotify/Figma link → nothing to embed");
                    return;
                }
                Debug.WriteLine($"Embed URL: {url}");
                Debug.WriteLine($"Embed text: '{url}'");

                // 4. Back on UI thread: splice into the TextBox
                await tb.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    var start = tb.SelectionStart;
                    var len = tb.SelectionLength;
                    var orig = tb.Text ?? string.Empty;

                    var updated = orig.Remove(start, len)
                                      .Insert(start, url);

                    tb.Text = updated;
                    tb.SelectionStart = start + url.Length;
                    tb.SelectionLength = 0;

                    Debug.WriteLine($"After insert – textLen={tb.Text.Length}, caret={tb.SelectionStart}");
                });
            }
            catch (Exception ex) {
                Debug.WriteLine($"EmbedLinkAsync failed: {ex}");
            }
        }

        private bool TryGetEmbedLink(string text, out string link) {
            Debug.WriteLine($">> TryGetEmbedLink('{text}')");
            link = null;

            // auto‐prepend HTTPS if missing
            if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                text = "https://" + text;
                Debug.WriteLine($"  → Prefixed scheme: '{text}'");
            }
            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri)) {
                Debug.WriteLine("  → Uri.TryCreate failed");
                return false;
            }

            var host = uri.Host.ToLowerInvariant();
            var segments = uri.AbsolutePath
                              .Trim('/')
                              .Split('/', StringSplitOptions.RemoveEmptyEntries);

            Debug.WriteLine($"  → host='{host}', segments=[{string.Join(", ", segments)}]");

            var isSpotify = host == "open.spotify.com"
                         && segments.Length > 0
                         && new[] { "track", "album", "playlist", "artist" }
                             .Contains(segments[0]);
            Debug.WriteLine($"  → isSpotify? {isSpotify}");

            var isFigma = (host == "figma.com" || host.EndsWith(".figma.com"))
                       && segments.Length > 0
                       && segments[0] == "design";
            Debug.WriteLine($"  → isFigma? {isFigma}");

            if (isSpotify) {
                link = "<iframe data-testid=\"embed-iframe\" style=\"border-radius:12px\" src=\"https://open.spotify.com/embed/" + segments[0] + "/" + segments[1] + "?utm_source=generator\" width=\"100%\" height=\"352\" frameBorder=\"0\" allowfullscreen=\"\" allow=\"autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture\" loading=\"lazy\"></iframe>";
            }
            if (isFigma) {
                link = "<iframe style=\"border: 1px solid rgba(0, 0, 0, 0.1);\" width=\"800\" height=\"450\" src=\"https://embed.figma.com/" + segments[0] + "/" + segments[1] + "\" allowfullscreen></iframe>";
            }

            var ok = isSpotify || isFigma;
            Debug.WriteLine($"<< TryGetEmbedLink returns {ok}");
            return ok;
        }

        private void InnerBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e) {
            UpdateSelectionProperties();
        }

        private void InnerBox_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            UpdateSelectionProperties();
        }

        public class CursorLineChangedEventArgs : EventArgs {
            public int LineStartIndex { get; }
            public int LineEndIndex { get; }
            public string LineText { get; }
            public int CurrentLineNumber { get; }
            public int PreviousLineNumber { get; }
            public int HeadingLevel { get; }

            public CursorLineChangedEventArgs(
                int lineStart,
                int lineEnd,
                string lineText,
                int currentLineNumber,
                int previousLineNumber,
                int headingLevel
            ) {
                LineStartIndex = lineStart;
                LineEndIndex = lineEnd;
                LineText = lineText;
                CurrentLineNumber = currentLineNumber;
                PreviousLineNumber = previousLineNumber;
                HeadingLevel = headingLevel;
            }
        }

        private void InnerBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (_updatingText)
                return;

            if (!string.IsNullOrEmpty(NoteID)) {
                NoteFileHandlingHelper.WriteNoteFileAsync(NoteID, Text);
            }

            UpdateSelectionProperties();
        }

        private void InnerBox_SelectionChanged(object sender, RoutedEventArgs e) {
            if (_innerBox == null)
                return;

            bool positionChanged = _innerBox.SelectionStart != _lastSelectionStart;
            bool lengthChanged = _innerBox.SelectionLength != _lastSelectionLength;

            if (positionChanged || lengthChanged) {
                Debug.WriteLine($"[#{_instanceId}] Selection changed: {_innerBox.SelectionStart} (was {_lastSelectionStart}), " +
                               $"len {_innerBox.SelectionLength} (was {_lastSelectionLength})");

                UpdateSelectionProperties();
                _lastSelectionStart = _innerBox.SelectionStart;
                _lastSelectionLength = _innerBox.SelectionLength;
            }
        }
        private void UpdateSelectionProperties() {
            if (_innerBox == null || _updatingSelection || _updatingText)
                return;

            try {
                _updatingSelection = true;
                bool hasSelection = _innerBox.SelectionLength > 0;
                SetValue(HasSelectionProperty, hasSelection);

                SetValue(IsSelectionBoldProperty, IsFormattedAtPosition("**", false));
                SetValue(IsSelectionItalicProperty, IsFormattedAtPosition("*", true));
                SetValue(IsSelectionStrikethroughProperty, IsFormattedAtPosition("~~", false));
                SetValue(IsSelectionCodeProperty, IsFormattedAtPosition("`", false));

                UpdateHeadingProperties();
            }
            finally {
                _updatingSelection = false;
            }
        }

        private void UpdateHeadingProperties() {
            if (_innerBox == null)
                return;

            try {
                _updatingHeadings = true;
                int cursorPosition = _innerBox.SelectionStart;
                (int lineStart, int lineEnd, string lineText, int lineNumber) = GetCurrentLine(cursorPosition);

                // Handle empty line case
                if (lineText == "" && cursorPosition < _innerBox.Text.Length &&
                    (_innerBox.Text[cursorPosition] == '\n' || _innerBox.Text[cursorPosition] == '\r')) {
                    (lineStart, lineEnd, lineText, lineNumber) = GetCurrentLine(cursorPosition + 1);
                }

                Debug.WriteLine($"[#{_instanceId}] Updating headings: cursor={cursorPosition}, lastLine={_lastCursorLineStart}, newLine={lineStart}");

                int headingLevel = 0;
                if (lineText.StartsWith("### "))
                    headingLevel = 3;
                else if (lineText.StartsWith("## "))
                    headingLevel = 2;
                else if (lineText.StartsWith("# "))
                    headingLevel = 1;

                // Detect bullet state
                bool isBullet = lineText.StartsWith("- ") || lineText.StartsWith("* ");
                SetValue(IsBulletedProperty, isBullet);

                SetValue(IsHeading1Property, headingLevel == 1);
                SetValue(IsHeading2Property, headingLevel == 2);
                SetValue(IsHeading3Property, headingLevel == 3);

                if (lineStart != _lastCursorLineStart) {
                    Debug.WriteLine($"[#{_instanceId}] Line changed: {_lastCursorLineNumber} -> {lineNumber}");

                    CursorLineChanged?.Invoke(this, new CursorLineChangedEventArgs(
                        lineStart,
                        lineEnd,
                        lineText,
                        lineNumber,
                        _lastCursorLineNumber,
                        headingLevel
                    ));

                    _lastCursorLineStart = lineStart;
                    _lastCursorLineNumber = lineNumber;
                }
            }
            finally {
                _updatingHeadings = false;
            }
        }
        private static void OnHeadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = (FileEditorBox)d;
            if (control._updatingHeadings || !(bool)e.NewValue)
                return;

            int level = e.Property switch {
                _ when e.Property == IsHeading1Property => 1,
                _ when e.Property == IsHeading2Property => 2,
                _ when e.Property == IsHeading3Property => 3,
                _ => 0
            };

            control.ToggleHeading(level);
        }

        private void ToggleHeading(int targetLevel) {
            if (_innerBox == null)
                return;

            try {
                _updatingText = true;
                int cursorPosition = _innerBox.SelectionStart;
                (int lineStart, int lineEnd, string lineText, int lineNumber) = GetCurrentLine(cursorPosition);

                // Remove any existing block formatting
                lineText = RemoveBlockFormatting(lineText);

                string newLineText;
                int headingPrefixLength = 0;

                // Check if already formatted with same heading level
                if (lineText.StartsWith(new string('#', targetLevel) + " ")) {
                    // Toggle off: remove heading
                    newLineText = lineText.Substring(targetLevel + 1);
                }
                else {
                    // Apply new heading
                    newLineText = new string('#', targetLevel) + " " + lineText.TrimStart();
                    headingPrefixLength = targetLevel + 1;
                }

                string newText = _innerBox.Text.Substring(0, lineStart) +
                                newLineText +
                                _innerBox.Text.Substring(lineEnd);

                Text = newText;
                int newCursorPosition = lineStart + headingPrefixLength;
                _innerBox.SelectionStart = Math.Min(newCursorPosition, newText.Length);
                _innerBox.SelectionLength = 0;
            }
            finally {
                _updatingText = false;
                UpdateHeadingProperties();
            }
        }

        private static void OnBulletPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = (FileEditorBox)d;
            if (control._updatingHeadings || !(bool)e.NewValue)
                return;

            control.ToggleBullet();
        }

        private void ToggleBullet() {
            if (_innerBox == null)
                return;

            try {
                _updatingText = true;
                int cursorPosition = _innerBox.SelectionStart;
                (int lineStart, int lineEnd, string lineText, int lineNumber) = GetCurrentLine(cursorPosition);

                // Remove any existing block formatting
                lineText = RemoveBlockFormatting(lineText);

                bool isBullet = lineText.StartsWith("- ") || lineText.StartsWith("* ");
                string newLineText;

                if (isBullet) {
                    // Remove bullet
                    newLineText = lineText.Substring(2);
                }
                else {
                    // Add bullet (using '-' as standard)
                    newLineText = "- " + lineText.TrimStart();
                }

                string newText = _innerBox.Text.Substring(0, lineStart) +
                                newLineText +
                                _innerBox.Text.Substring(lineEnd);

                Text = newText;
                int newCursorPosition = cursorPosition + (isBullet ? -2 : 2);
                _innerBox.SelectionStart = Math.Max(0, Math.Min(newCursorPosition, newText.Length));
                _innerBox.SelectionLength = 0;
            }
            finally {
                _updatingText = false;
                UpdateHeadingProperties();
            }
        }

        private static string RemoveBlockFormatting(string lineText) {
            // Remove any heading markers
            if (lineText.StartsWith("#")) {
                int i = 0;
                while (i < lineText.Length && lineText[i] == '#')
                    i++;
                if (i < lineText.Length && lineText[i] == ' ') {
                    lineText = lineText.Substring(i + 1);
                }
                else if (i < lineText.Length) {
                    // Handle case where # isn't followed by space
                    lineText = lineText.Substring(i);
                }
            }

            // Remove any bullet markers
            if (lineText.StartsWith("- ")) {
                lineText = lineText.Substring(2);
            }
            else if (lineText.StartsWith("* ")) {
                lineText = lineText.Substring(2);
            }

            return lineText;
        }
        private (int start, int end, string text, int number) GetCurrentLine(int position) {
            string text = _innerBox.Text ?? "";
            if (string.IsNullOrEmpty(text))
                return (0, 0, "", 0);

            position = Math.Clamp(position, 0, text.Length);
            int lineStart = position;
            int lineEnd = position;

            for (int i = position; i >= 0; i--) {
                if (i == 0) {
                    lineStart = 0;
                    break;
                }

                if (text[i - 1] == '\n' || text[i - 1] == '\r') {
                    lineStart = i;
                    break;
                }
            }

            for (int i = position; i <= text.Length; i++) {
                if (i == text.Length || text[i] == '\n' || text[i] == '\r') {
                    lineEnd = i;
                    break;
                }
            }

            int lineNumber = 1;
            for (int i = 0; i < lineStart; i++) {
                if (text[i] == '\n' || text[i] == '\r') {
                    lineNumber++;
                }
            }

            int length = lineEnd - lineStart;
            string lineText = length > 0 ? text.Substring(lineStart, length) : "";

            Debug.WriteLine($"[#{_instanceId}] GetCurrentLine: pos={position}, start={lineStart}, end={lineEnd}, line={lineNumber}, text='{lineText}'");
            return (lineStart, lineEnd, lineText, lineNumber);
        }

        private bool IsFormattedAtPosition(string pattern, bool isItalic) {
            if (_innerBox == null || string.IsNullOrEmpty(_innerBox.Text))
                return false;

            int pos = _innerBox.SelectionStart;
            int len = _innerBox.SelectionLength;
            string text = _innerBox.Text;
            int patternLength = pattern.Length;

            // Special handling for italic to prevent confusion with bold
            if (isItalic && pattern == "*") {
                // Check if we're in the middle of bold formatting
                if (pos > 0 && pos < text.Length - 1 &&
                    text[pos - 1] == '*' && text[pos] == '*') {
                    return false;
                }
            }

            // For selections, check exact boundaries
            if (len > 0) {
                return CheckExactBoundaries(text, pos, len, pattern);
            }

            // For cursor position, check if we're within matching markers
            return IsWithinMarkers(text, pos, pattern, isItalic);
        }

        private bool CheckExactBoundaries(string text, int pos, int len, string pattern) {
            // Check left boundary
            bool leftMatch = pos >= pattern.Length &&
                           text.Substring(pos - pattern.Length, pattern.Length) == pattern;

            // Check right boundary
            bool rightMatch = (pos + len + pattern.Length) <= text.Length &&
                            text.Substring(pos + len, pattern.Length) == pattern;

            return leftMatch && rightMatch;
        }

        private bool IsWithinMarkers(string text, int pos, string pattern, bool isItalic) {
            int patternLength = pattern.Length;
            int openIndex = -1;
            int closeIndex = -1;

            // Find opening marker
            for (int i = Math.Min(pos, text.Length - patternLength); i >= 0; i--) {
                if (text.Substring(i, patternLength) == pattern) {
                    // Skip if part of bold pattern for italics
                    if (isItalic && pattern == "*") {
                        if (i > 0 && text[i - 1] == '*')
                            continue; // Left of bold
                        if (i < text.Length - 1 && text[i + 1] == '*')
                            continue; // Right of bold
                    }
                    // Skip escaped markers
                    if (i > 0 && text[i - 1] == '\\')
                        continue;

                    openIndex = i;
                    break;
                }
            }

            if (openIndex == -1)
                return false;

            // Find closing marker
            for (int i = Math.Max(openIndex + patternLength, pos); i <= text.Length - patternLength; i++) {
                if (text.Substring(i, patternLength) == pattern) {
                    // Skip if part of bold pattern for italics
                    if (isItalic && pattern == "*") {
                        if (i > 0 && text[i - 1] == '*')
                            continue; // Left of bold
                        if (i < text.Length - 1 && text[i + 1] == '*')
                            continue; // Right of bold
                    }
                    // Skip escaped markers
                    if (i > 0 && text[i - 1] == '\\')
                        continue;

                    closeIndex = i;
                    break;
                }
            }

            return closeIndex != -1 &&
                   pos > openIndex &&
                   pos <= closeIndex;
        }
        private void ToggleFormatting(string pattern, bool isItalic) {
            if (_innerBox == null)
                return;

            int start = _innerBox.SelectionStart;
            int length = _innerBox.SelectionLength;
            string text = _innerBox.Text;
            int patternLength = pattern.Length;

            try {
                _updatingText = true;
                string newText = text;
                int newStart = start;
                int newLength = length;

                bool isCurrentlyFormatted = IsFormattedAtPosition(pattern, isItalic);

                if (isCurrentlyFormatted) {
                    // Remove formatting
                    if (length > 0) {
                        // Remove markers around selection
                        newText = text.Remove(start + length, patternLength)
                                     .Remove(start - patternLength, patternLength);
                        newStart = start - patternLength;
                        newLength = length;
                    }
                    else {
                        // Remove markers around cursor position
                        int openIndex = FindNearestOpeningMarker(text, pattern, start, isItalic);
                        int closeIndex = FindNearestClosingMarker(text, pattern, start, isItalic);

                        if (openIndex != -1 && closeIndex != -1) {
                            newText = text.Remove(closeIndex, patternLength)
                                         .Remove(openIndex, patternLength);
                            newStart = openIndex;
                        }
                    }
                }
                else {
                    // Add formatting
                    if (length > 0) {
                        // Apply to selection
                        string selectedText = text.Substring(start, length);
                        string formattedText = pattern + selectedText + pattern;
                        newText = text.Remove(start, length).Insert(start, formattedText);
                        newStart = start + patternLength;
                        newLength = length;
                    }
                    else {
                        // Empty selection - insert balanced markers
                        newText = text.Insert(start, pattern + pattern);
                        newStart = start + patternLength; // Place cursor between markers
                    }
                }

                Text = newText;
                _innerBox.SelectionStart = newStart;
                _innerBox.SelectionLength = newLength;
            }
            finally {
                _updatingText = false;
                UpdateSelectionProperties();
            }
        }

        private int FindNearestOpeningMarker(string text, string pattern, int position, bool isItalic) {
            int closest = -1;
            for (int i = 0; i <= position; i++) {
                if (i + pattern.Length > text.Length)
                    continue;
                if (text.Substring(i, pattern.Length) == pattern) {
                    // Skip if part of bold pattern for italics
                    if (isItalic && pattern == "*") {
                        if (i > 0 && text[i - 1] == '*')
                            continue;
                        if (i < text.Length - 1 && text[i + 1] == '*')
                            continue;
                    }
                    // Skip escaped markers
                    if (i > 0 && text[i - 1] == '\\')
                        continue;
                    closest = i;
                }
            }
            return closest;
        }

        private int FindNearestClosingMarker(string text, string pattern, int position, bool isItalic) {
            int closest = -1;
            for (int i = position; i <= text.Length - pattern.Length; i++) {
                if (text.Substring(i, pattern.Length) == pattern) {
                    // Skip if part of bold pattern for italics
                    if (isItalic && pattern == "*") {
                        if (i > 0 && text[i - 1] == '*')
                            continue;
                        if (i < text.Length - 1 && text[i + 1] == '*')
                            continue;
                    }
                    // Skip escaped markers
                    if (i > 0 && text[i - 1] == '\\')
                        continue;
                    if (closest == -1 || Math.Abs(i - position) < Math.Abs(closest - position)) {
                        closest = i;
                    }
                }
            }
            return closest;
        }

        private static void OnFormattingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = (FileEditorBox)d;
            if (control._updatingSelection)
                return;

            string pattern = e.Property switch {
                _ when e.Property == IsSelectionBoldProperty => "**",
                _ when e.Property == IsSelectionItalicProperty => "*",
                _ when e.Property == IsSelectionStrikethroughProperty => "~~",
                _ when e.Property == IsSelectionCodeProperty => "`",
                _ => null
            };

            if (pattern != null) {
                bool isItalic = pattern == "*";
                bool isFormatted = control.IsFormattedAtPosition(pattern, isItalic);

                if ((bool)e.NewValue != isFormatted) {
                    control.ToggleFormatting(pattern, isItalic);
                }
            }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        }

        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string NoteID {
            get => (string)GetValue(NoteIDProperty);
            set => SetValue(NoteIDProperty, value);
        }

        public bool IsSelectionBold {
            get => (bool)GetValue(IsSelectionBoldProperty);
            set => SetValue(IsSelectionBoldProperty, value);
        }

        public bool IsSelectionItalic {
            get => (bool)GetValue(IsSelectionItalicProperty);
            set => SetValue(IsSelectionItalicProperty, value);
        }

        public bool IsSelectionStrikethrough {
            get => (bool)GetValue(IsSelectionStrikethroughProperty);
            set => SetValue(IsSelectionStrikethroughProperty, value);
        }

        public bool IsSelectionCode {
            get => (bool)GetValue(IsSelectionCodeProperty);
            set => SetValue(IsSelectionCodeProperty, value);
        }

        public bool IsBulleted {
            get => (bool)GetValue(IsBulletedProperty);
            set => SetValue(IsBulletedProperty, value);
        }

        public bool IsHeading1 {
            get => (bool)GetValue(IsHeading1Property);
            set => SetValue(IsHeading1Property, value);
        }

        public bool IsHeading2 {
            get => (bool)GetValue(IsHeading2Property);
            set => SetValue(IsHeading2Property, value);
        }

        public bool IsHeading3 {
            get => (bool)GetValue(IsHeading3Property);
            set => SetValue(IsHeading3Property, value);
        }

        public bool HasSelection {
            get => (bool)GetValue(HasSelectionProperty);
        }
    }
}
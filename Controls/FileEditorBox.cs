using Fairmark.Helpers;
using System;
using System.Buffers.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Fairmark.Controls
{
    public sealed class FileEditorBox : Control, INotifyPropertyChanged
    {
        private TextBox _innerBox;
        private int _instanceId;
        private static int _instanceCounter;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FileEditorBox),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty NoteIDProperty =
            DependencyProperty.Register("NoteID", typeof(string), typeof(FileEditorBox),
                new PropertyMetadata(string.Empty));

        public ICommand BoldCommand { get; }
        public ICommand ItalicCommand { get; }
        public ICommand StrikethroughCommand { get; }
        public ICommand CodeCommand { get; }
        public ICommand BulletCommand { get; }
        public ICommand QuoteCommand { get; }
        public ICommand Heading1Command { get; }
        public ICommand Heading2Command { get; }
        public ICommand Heading3Command { get; }
        public ICommand HorizontalLineCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }

        private int _wordCount;
        private int _characterCount;
        public event PropertyChangedEventHandler PropertyChanged;

        public int WordCount
        {
            get => _wordCount;
            private set
            {
                if (_wordCount != value)
                {
                    _wordCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WordCount)));
                }
            }
        }
        public int CharacterCount
        {
            get => _characterCount;
            private set
            {
                if (_characterCount != value)
                {
                    _characterCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharacterCount)));
                }
            }
        }

        public FileEditorBox()
        {
            _instanceId = ++_instanceCounter;
            Debug.WriteLine($"[#{_instanceId}] FileEditorBox created");
            this.DefaultStyleKey = typeof(FileEditorBox);
            this.Loaded += (s, e) => Debug.WriteLine($"[#{_instanceId}] Control loaded");

            BoldCommand = new RelayCommand(_ => ExecuteBoldCommand());
            ItalicCommand = new RelayCommand(_ => ExecuteItalicCommand());
            StrikethroughCommand = new RelayCommand(_ => ExecuteStrikethroughCommand());
            CodeCommand = new RelayCommand(_ => ExecuteCodeCommand());
            BulletCommand = new RelayCommand(_ => ExecuteBulletCommand());
            QuoteCommand = new RelayCommand(_ => ExecuteQuoteCommand());
            Heading1Command = new RelayCommand(_ => ExecuteHeadingCommand(1));
            Heading2Command = new RelayCommand(_ => ExecuteHeadingCommand(2));
            Heading3Command = new RelayCommand(_ => ExecuteHeadingCommand(3));
            HorizontalLineCommand = new RelayCommand(_ => ExecuteHorizontalLineCommand());
            UndoCommand = new RelayCommand(_ => ExecuteUndoCommand());
            RedoCommand = new RelayCommand(_ => ExecuteRedoCommand());
            CutCommand = new RelayCommand(_ => ExecuteCutCommand());
            CopyCommand = new RelayCommand(_ => ExecuteCopyCommand());
            PasteCommand = new RelayCommand(_ => ExecutePasteCommand());

            UpdateCounts();
        }

        private void ExecuteUndoCommand() => _innerBox?.Undo();
        private void ExecuteRedoCommand() => _innerBox?.Redo();
        private void ExecuteCutCommand() => _innerBox?.CutSelectionToClipboard();
        private void ExecuteCopyCommand() => _innerBox?.CopySelectionToClipboard();
        private void ExecutePasteCommand() => _innerBox?.PasteFromClipboard();

        private void ExecuteBoldCommand()
        {
            if (_innerBox == null || _innerBox.SelectionLength == 0) return;
            ToggleFormattingDynamic("**", false);
        }
        private void ExecuteItalicCommand()
        {
            if (_innerBox == null || _innerBox.SelectionLength == 0) return;
            ToggleFormattingDynamic("_", true);
        }
        private void ExecuteStrikethroughCommand()
        {
            if (_innerBox == null || _innerBox.SelectionLength == 0) return;
            ToggleFormattingDynamic("~~", false);
        }
        private void ExecuteCodeCommand()
        {
            if (_innerBox == null || _innerBox.SelectionLength == 0) return;
            ToggleFormattingDynamic("`", false);
        }
        private void ExecuteBulletCommand() => ToggleBulletDynamic();
        private void ExecuteQuoteCommand() => ToggleQuote();
        private void ExecuteHeadingCommand(int level) => ToggleHeadingDynamic(level);
        private void ExecuteHorizontalLineCommand() => AddNewHorizontalLine();

        protected override void OnApplyTemplate()
        {
            Debug.WriteLine($"[#{_instanceId}] Applying template");
            base.OnApplyTemplate();

            _innerBox = GetTemplateChild("MarkEditor") as TextBox;

            if (_innerBox != null)
            {
                Debug.WriteLine($"[#{_instanceId}] Inner box found");
                var binding = new Binding
                {
                    Source = this,
                    Path = new PropertyPath(nameof(Text)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                _innerBox.SetBinding(TextBox.TextProperty, binding);

                _innerBox.TextChanged += InnerBox_TextChanged;
                _innerBox.Paste += InnerBox_Paste;
            }
            else
            {
                Debug.WriteLine($"[#{_instanceId}] ERROR: Inner box not found!");
            }
        }
        private void InnerBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCounts();
            if (!string.IsNullOrEmpty(NoteID))
            {
                _ = NoteFileHandlingHelper.WriteNoteFileAsync(NoteID, Text);
            }
        }
        private void InnerBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            Debug.WriteLine(">> InnerBox_Paste fired");
            var settings = Application.Current.Resources["Settings"] as Settings;
            Debug.WriteLine($"AutoEmbed setting: {settings?.AutoEmbed}");
            if (settings?.AutoEmbed != true)
            {
                Debug.WriteLine("AutoEmbed disabled → normal paste");
                return;
            }
            var dp = Clipboard.GetContent();
            if (!dp.Contains(StandardDataFormats.Text))
            {
                Debug.WriteLine("No text on clipboard → normal paste");
                return;
            }
            e.Handled = true;
            Debug.WriteLine("Default paste canceled (e.Handled = true)");
            if (sender is TextBox tb)
            {
                _ = EmbedLinkAsync(tb);
            }
            else
            {
                Debug.WriteLine("Sender isn’t a TextBox – can’t embed");
            }
            Debug.WriteLine("<< InnerBox_Paste exit");
        }

        private void ToggleQuote()
        {
            if (_innerBox == null) return;
            int selectionStart = _innerBox.SelectionStart;
            int selectionLength = _innerBox.SelectionLength;
            string text = _innerBox.Text ?? string.Empty;
            int selStart = selectionStart;
            int selEnd = selectionStart + selectionLength;
            int blockStart = text.LastIndexOfAny(new[] { '\r', '\n' }, Math.Max(0, selStart - 1));
            blockStart = blockStart == -1 ? 0 : blockStart + 1;
            int blockEnd = text.IndexOfAny(new[] { '\r', '\n' }, selEnd);
            if (blockEnd == -1) blockEnd = text.Length;
            string selectedBlock = text.Substring(blockStart, blockEnd - blockStart);
            var lines = selectedBlock.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            bool allQuoted = lines.All(l => l.StartsWith("> "));
            var newLines = allQuoted
                ? lines.Select(l => l.StartsWith("> ") ? l.Substring(2) : l)
                : lines.Select(l => l.Length == 0 ? "> " : l.StartsWith("> ") ? l : "> " + l);
            string lineEnding = "\n";
            int rn = selectedBlock.IndexOf("\r\n");
            int r = selectedBlock.IndexOf('\r');
            int n = selectedBlock.IndexOf('\n');
            if (rn != -1) lineEnding = "\r\n";
            else if (r != -1 && (n == -1 || r < n)) lineEnding = "\r";
            else if (n != -1) lineEnding = "\n";
            string newBlock = string.Join(lineEnding, newLines);
            string newText = text.Substring(0, blockStart) + newBlock + text.Substring(blockEnd);
            int newSelectionStart = selectionStart + (allQuoted ? -2 : 2);
            int newSelectionLength = newBlock.Length - selectedBlock.Length + selectionLength;
            Text = newText;
            _innerBox.SelectionStart = Math.Max(0, Math.Min(newSelectionStart, newText.Length));
            _innerBox.SelectionLength = Math.Max(0, Math.Min(newSelectionLength, newText.Length - _innerBox.SelectionStart));
        }
        private void ToggleBulletDynamic()
        {
            if (_innerBox == null) return;
            int cursorPosition = _innerBox.SelectionStart;
            (int lineStart, int lineEnd, string lineText, _) = GetCurrentLine(cursorPosition);
            if (lineText.StartsWith("#"))
            {
                int i = 0;
                while (i < lineText.Length && lineText[i] == '#') i++;
                if (i < lineText.Length && lineText[i] == ' ')
                {
                    lineText = lineText.Substring(i + 1);
                }
                else if (i < lineText.Length)
                {
                    lineText = lineText.Substring(i);
                }
            }
            bool isBullet = lineText.StartsWith("- ") || lineText.StartsWith("* ");
            string newLineText = isBullet ? lineText.Substring(2) : "- " + lineText.TrimStart();
            string newText = _innerBox.Text.Substring(0, lineStart) + newLineText + _innerBox.Text.Substring(lineEnd);
            Text = newText;
            int newCursorPosition = cursorPosition + (isBullet ? -2 : 2);
            _innerBox.SelectionStart = Math.Max(0, Math.Min(newCursorPosition, newText.Length));
            _innerBox.SelectionLength = 0;
        }
        private void ToggleHeadingDynamic(int targetLevel)
        {
            if (_innerBox == null) return;
            int cursorPosition = _innerBox.SelectionStart;
            (int lineStart, int lineEnd, string lineText, _) = GetCurrentLine(cursorPosition);
            if (lineText.StartsWith("- "))
            {
                lineText = lineText.Substring(2);
            }
            else if (lineText.StartsWith("* "))
            {
                lineText = lineText.Substring(2);
            }
            lineText = RemoveBlockFormatting(lineText);
            string newLineText;
            int headingPrefixLength = 0;
            if (lineText.StartsWith(new string('#', targetLevel) + " "))
            {
                newLineText = lineText.Substring(targetLevel + 1);
            }
            else
            {
                newLineText = new string('#', targetLevel) + " " + lineText.TrimStart();
                headingPrefixLength = targetLevel + 1;
            }
            string newText = _innerBox.Text.Substring(0, lineStart) + newLineText + _innerBox.Text.Substring(lineEnd);
            Text = newText;
            int newCursorPosition = lineStart + headingPrefixLength;
            _innerBox.SelectionStart = Math.Min(newCursorPosition, newText.Length);
            _innerBox.SelectionLength = 0;
        }
        private void AddNewHorizontalLine()
        {
            if (_innerBox == null) return;
            int cursorPosition = _innerBox.SelectionStart;
            (_, int lineEnd, _, _) = GetCurrentLine(cursorPosition);
            int insertPosition = lineEnd;
            string newText = _innerBox.Text.Insert(insertPosition, "\n---\n");
            Text = newText;
            _innerBox.SelectionStart = insertPosition + "\n---\n".Length;
            _innerBox.SelectionLength = 0;
        }
        private void ToggleFormattingDynamic(string pattern, bool isItalic)
        {
            if (_innerBox == null)
                return;
            int start = _innerBox.SelectionStart;
            int length = _innerBox.SelectionLength;
            string text = _innerBox.Text;
            _ = pattern.Length;
            bool isCurrentlyFormatted = IsFormattedAtPosition(pattern, isItalic, text, start, length);
            ToggleFormatting(pattern, isItalic, !isCurrentlyFormatted, text, start, length);
        }
        private void ToggleFormatting(string pattern, bool isItalic, bool apply, string text, int start, int length)
        {
            int patternLength = pattern.Length;
            string newText = text;
            int newStart = start;
            int newLength = length;
            RemoveAllMarkersFromSelection(ref newText, ref newStart, ref newLength);
            if (apply)
            {
                if (newLength > 0)
                {
                    string selectedText = newText.Substring(newStart, newLength);
                    string formattedText = pattern + selectedText + pattern;
                    newText = newText.Remove(newStart, newLength).Insert(newStart, formattedText);
                    newStart = newStart + patternLength;
                    newLength = newLength;
                }
                else
                {
                    newText = newText.Insert(newStart, pattern + pattern);
                    newStart = newStart + patternLength;
                }
            }
            else
            {
                int left = newStart;
                int right = newStart + newLength;
                while (left - patternLength >= 0 && newText.Substring(left - patternLength, patternLength) == pattern)
                {
                    left -= patternLength;
                }
                while (right + patternLength <= newText.Length && newText.Substring(right, patternLength) == pattern)
                {
                    right += patternLength;
                }
                if (left + patternLength <= right - patternLength &&
                    newText.Substring(left, patternLength) == pattern &&
                    newText.Substring(right - patternLength, patternLength) == pattern)
                {
                    newText = newText.Remove(right - patternLength, patternLength)
                                     .Remove(left, patternLength);
                    newStart = left;
                    newLength = right - left - (2 * patternLength);
                    if (newLength < 0) newLength = 0;
                }
                else
                {
                    if (newLength > 0)
                    {
                        newText = newText.Remove(newStart + newLength, patternLength)
                                         .Remove(newStart - patternLength, patternLength);
                        newStart = newStart - patternLength;
                        newLength = newLength;
                    }
                    else
                    {
                        int openIndex = FindNearestOpeningMarker(newText, pattern, newStart, isItalic);
                        int closeIndex = FindNearestClosingMarker(newText, pattern, newStart, isItalic);
                        if (openIndex != -1 && closeIndex != -1)
                        {
                            newText = newText.Remove(closeIndex, patternLength)
                                             .Remove(openIndex, patternLength);
                            newStart = openIndex;
                        }
                    }
                }
            }
            RemoveAllMarkersFromSelection(ref newText, ref newStart, ref newLength);
            Text = newText;
            _innerBox.SelectionStart = newStart;
            _innerBox.SelectionLength = newLength;
        }
        private void RemoveAllMarkersFromSelection(ref string text, ref int start, ref int length)
        {
            string[] markers = { "**", "*", "~~", "`", "__", "_" };
            bool changed;
            do
            {
                changed = false;
                foreach (var marker in markers.OrderByDescending(m => m.Length))
                {
                    int markerLen = marker.Length;
                    while (length > 0 && start + markerLen <= text.Length && text.Substring(start, markerLen) == marker)
                    {
                        text = text.Remove(start, markerLen);
                        length -= markerLen;
                        changed = true;
                    }
                    while (length > 0 && start + length - markerLen >= 0 && start + length <= text.Length && text.Substring(start + length - markerLen, markerLen) == marker)
                    {
                        text = text.Remove(start + length - markerLen, markerLen);
                        length -= markerLen;
                        changed = true;
                    }
                }
            } while (changed);
        }
        private bool IsFormattedAtPosition(string pattern, bool isItalic, string text, int pos, int len)
        {
            int patternLength = pattern.Length;
            if (isItalic && pattern == "*")
            {
                if (pos > 0 && pos < text.Length - 1 && text[pos - 1] == '*' && text[pos] == '*')
                {
                    return false;
                }
            }
            if (len > 0)
            {
                bool leftMatch = pos >= patternLength && text.Substring(pos - patternLength, patternLength) == pattern;
                bool rightMatch = (pos + len + patternLength) <= text.Length && text.Substring(pos + len, patternLength) == pattern;
                return leftMatch && rightMatch;
            }
            int openIndex = -1;
            int closeIndex = -1;
            for (int i = Math.Min(pos, text.Length - patternLength); i >= 0; i--)
            {
                if (text.Substring(i, patternLength) == pattern)
                {
                    if (isItalic && pattern == "*")
                    {
                        if (i > 0 && text[i - 1] == '*') continue;
                        if (i < text.Length - 1 && text[i + 1] == '*') continue;
                    }
                    if (i > 0 && text[i - 1] == '\\') continue;
                    openIndex = i;
                    break;
                }
            }
            if (openIndex == -1) return false;
            for (int i = Math.Max(openIndex + patternLength, pos); i <= text.Length - patternLength; i++)
            {
                if (text.Substring(i, patternLength) == pattern)
                {
                    if (isItalic && pattern == "*")
                    {
                        if (i > 0 && text[i - 1] == '*') continue;
                        if (i < text.Length - 1 && text[i + 1] == '*') continue;
                    }
                    if (i > 0 && text[i - 1] == '\\') continue;
                    closeIndex = i;
                    break;
                }
            }
            return closeIndex != -1 && pos > openIndex && pos <= closeIndex;
        }
        private int FindNearestOpeningMarker(string text, string pattern, int position, bool isItalic)
        {
            int closest = -1;
            for (int i = 0; i <= position; i++)
            {
                if (i + pattern.Length > text.Length) continue;
                if (text.Substring(i, pattern.Length) == pattern)
                {
                    if (isItalic && pattern == "*")
                    {
                        if (i > 0 && text[i - 1] == '*') continue;
                        if (i < text.Length - 1 && text[i + 1] == '*') continue;
                    }
                    if (i > 0 && text[i - 1] == '\\') continue;
                    closest = i;
                }
            }
            return closest;
        }
        public void InsertLink(string url, string displayText = null)
        {
            if (_innerBox == null) return;
            int start = _innerBox.SelectionStart;
            int length = _innerBox.SelectionLength;
            string text = _innerBox.Text ?? string.Empty;
            if (string.IsNullOrEmpty(displayText))
            {
                displayText = url;
            }
            string linkText = $"[{displayText}]({url})";
            if (length > 0)
            {
                text = text.Remove(start, length).Insert(start, linkText);
                _innerBox.SelectionStart = start + linkText.Length;
                _innerBox.SelectionLength = 0;
            }
            else
            {
                text = text.Insert(start, linkText);
                _innerBox.SelectionStart = start + linkText.Length;
                _innerBox.SelectionLength = 0;
            }
            Text = text;
        }
        public async Task InsertImage(string selectedImage)
        {
            if (!((await (new ImageFolderHelper()).GetImageList()).Any(t => t.Name == selectedImage))) return;
            if (_innerBox == null) return;
            int start = _innerBox.SelectionStart;
            int length = _innerBox.SelectionLength;
            string text = _innerBox.Text ?? string.Empty;
            string instext = $"\n![image](local:///{Uri.EscapeUriString(selectedImage)})\n";
            if (length > 0)
            {
                text = text.Remove(start, length).Insert(start, instext);
                _innerBox.SelectionStart = start + instext.Length;
                _innerBox.SelectionLength = 0;
            }
            else
            {
                text = text.Insert(start, instext);
                _innerBox.SelectionStart = start + instext.Length;
                _innerBox.SelectionLength = 0;
            }
            _innerBox.Text = text;
        }
        private int FindNearestClosingMarker(string text, string pattern, int position, bool isItalic)
        {
            int closest = -1;
            for (int i = position; i <= text.Length - pattern.Length; i++)
            {
                if (text.Substring(i, pattern.Length) == pattern)
                {
                    if (isItalic && pattern == "*")
                    {
                        if (i > 0 && text[i - 1] == '*') continue;
                        if (i < text.Length - 1 && text[i + 1] == '*') continue;
                    }
                    if (i > 0 && text[i - 1] == '\\') continue;
                    if (closest == -1 || Math.Abs(i - position) < Math.Abs(closest - position))
                    {
                        closest = i;
                    }
                }
            }
            return closest;
        }
        private static string RemoveBlockFormatting(string lineText)
        {
            if (lineText.StartsWith("#"))
            {
                int i = 0;
                while (i < lineText.Length && lineText[i] == '#') i++;
                if (i < lineText.Length && lineText[i] == ' ')
                {
                    lineText = lineText.Substring(i + 1);
                }
                else if (i < lineText.Length)
                {
                    lineText = lineText.Substring(i);
                }
            }
            if (lineText.StartsWith("- "))
            {
                lineText = lineText.Substring(2);
            }
            else if (lineText.StartsWith("* "))
            {
                lineText = lineText.Substring(2);
            }
            return lineText;
        }
        private (int start, int end, string text, int number) GetCurrentLine(int position)
        {
            string text = _innerBox.Text ?? "";
            if (string.IsNullOrEmpty(text)) return (0, 0, "", 0);
            position = Math.Clamp(position, 0, text.Length);
            int lineStart = position;
            int lineEnd = position;
            for (int i = position; i >= 0; i--)
            {
                if (i == 0) { lineStart = 0; break; }
                if (text[i - 1] == '\n' || text[i - 1] == '\r') { lineStart = i; break; }
            }
            for (int i = position; i <= text.Length; i++)
            {
                if (i == text.Length || text[i] == '\n' || text[i] == '\r') { lineEnd = i; break; }
            }
            int lineNumber = 1;
            for (int i = 0; i < lineStart; i++)
            {
                if (text[i] == '\n' || text[i] == '\r') { lineNumber++; }
            }
            int length = lineEnd - lineStart;
            string lineText = length > 0 ? text.Substring(lineStart, length) : "";
            Debug.WriteLine($"[#{_instanceId}] GetCurrentLine: pos={position}, start={lineStart}, end={lineEnd}, line={lineNumber}, text='{lineText}'");
            return (lineStart, lineEnd, lineText, lineNumber);
        }
        private async Task EmbedLinkAsync(TextBox tb)
        {
            try
            {
                var dp = Clipboard.GetContent();
                var raw = (await dp.GetTextAsync()).Trim();
                Debug.WriteLine($"Clipboard text: '{raw}'");
                if (string.IsNullOrWhiteSpace(raw))
                {
                    Debug.WriteLine("Empty/whitespace → nothing to embed");
                    return;
                }
                if (!TryGetEmbedLink(raw, out var url))
                {
                    Debug.WriteLine("Not a Spotify/Figma link → nothing to embed");
                    return;
                }
                Debug.WriteLine($"Embed URL: {url}");
                Debug.WriteLine($"Embed text: '{url}'");
                await tb.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
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
            catch (Exception ex)
            {
                Debug.WriteLine($"EmbedLinkAsync failed: {ex}");
            }
        }
        private bool TryGetEmbedLink(string text, out string link)
        {
            Debug.WriteLine($">> TryGetEmbedLink('{text}')");
            link = null;
            if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                text = "https://" + text;
                Debug.WriteLine($"  → Prefixed scheme: '{text}'");
            }
            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
            {
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
            if (isSpotify)
            {
                link = "<iframe data-testid=\"embed-iframe\" style=\"border-radius:12px\" src=\"https://open.spotify.com/embed/" + segments[0] + "/" + segments[1] + "?utm_source=generator\" width=\"100%\" height=\"352\" frameBorder=\"0\" allowfullscreen=\"\" allow=\"autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture\" loading=\"lazy\"></iframe>";
            }
            if (isFigma)
            {
                link = "<iframe style=\"border: 1px solid rgba(0, 0, 0, 0.1);\" width=\"800\" height=\"450\" src=\"https://embed.figma.com/" + segments[0] + "/" + segments[1] + "\" allowfullscreen></iframe>";
            }
            var ok = isSpotify || isFigma;
            Debug.WriteLine($"<< TryGetEmbedLink returns {ok}");
            return ok;
        }
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                UpdateCounts();
            }
        }
        public string NoteID
        {
            get => (string)GetValue(NoteIDProperty);
            set => SetValue(NoteIDProperty, value);
        }
        private void UpdateCounts()
        {
            var text = Text ?? string.Empty;
            CharacterCount = text.Length;
            WordCount = string.IsNullOrWhiteSpace(text) ? 0 : text.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries).Length;
        }

        internal void InsertDetails(string summary, string details)
        {
            if (_innerBox == null) return;
            int start = _innerBox.SelectionStart;
            int length = _innerBox.SelectionLength;
            string text = _innerBox.Text ?? string.Empty;
            string instext = $"""
            <details>
            <summary>
            {summary}
            </summary>
            {details}
            </details>
            """;
            if (length > 0)
            {
                text = text.Remove(start, length).Insert(start, instext);
                _innerBox.SelectionStart = start + instext.Length;
                _innerBox.SelectionLength = 0;
            }
            else
            {
                text = text.Insert(start, instext);
                _innerBox.SelectionStart = start + instext.Length;
                _innerBox.SelectionLength = 0;
            }
            Text = text;
        }
    }
}
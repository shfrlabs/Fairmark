using Fairmark.Helpers;
using Markdig;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
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
        private bool _isUpdatingText;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FileEditorBox),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty NoteIDProperty =
            DependencyProperty.Register("NoteID", typeof(string), typeof(FileEditorBox),
                new PropertyMetadata(string.Empty));

        public RelayCommand BoldCommand { get; private set; }
        public RelayCommand ItalicCommand { get; private set; }
        public RelayCommand StrikethroughCommand { get; private set; }
        public RelayCommand CodeCommand { get; private set; }
        public RelayCommand BulletCommand { get; private set; }
        public RelayCommand QuoteCommand { get; private set; }
        public RelayCommand Heading1Command { get; private set; }
        public RelayCommand Heading2Command { get; private set; }
        public RelayCommand Heading3Command { get; private set; }
        public RelayCommand HorizontalLineCommand { get; private set; }
        public RelayCommand UndoCommand { get; private set; }
        public RelayCommand RedoCommand { get; private set; }
        public RelayCommand CutCommand { get; private set; }
        public RelayCommand CopyCommand { get; private set; }
        public RelayCommand PasteCommand { get; private set; }

        private int _wordCount;
        private int _characterCount;
        private bool _hasSelection;
        private bool _canUndo;
        private bool _canRedo;

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

        public bool HasSelection
        {
            get => _hasSelection;
            private set
            {
                if (_hasSelection != value)
                {
                    _hasSelection = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSelection)));
                }
            }
        }

        public bool CanUndo
        {
            get => _canUndo;
            private set
            {
                if (_canUndo != value)
                {
                    _canUndo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanUndo)));
                    UndoCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanRedo
        {
            get => _canRedo;
            private set
            {
                if (_canRedo != value)
                {
                    _canRedo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRedo)));
                    RedoCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public FileEditorBox()
        {
            _instanceId = ++_instanceCounter;
            Debug.WriteLine($"[#{_instanceId}] FileEditorBox created");
            this.DefaultStyleKey = typeof(FileEditorBox);
            this.Loaded += (s, e) => Debug.WriteLine($"[#{_instanceId}] Control loaded");

            BoldCommand = new RelayCommand(_ => ExecuteBoldCommand(), _ => true);
            ItalicCommand = new RelayCommand(_ => ExecuteItalicCommand(), _ => true);
            StrikethroughCommand = new RelayCommand(_ => ExecuteStrikethroughCommand(), _ => true);
            CodeCommand = new RelayCommand(_ => ExecuteCodeCommand(), _ => true);
            BulletCommand = new RelayCommand(_ => ExecuteBulletCommand(), _ => true);
            QuoteCommand = new RelayCommand(_ => ExecuteQuoteCommand(), _ => true);
            Heading1Command = new RelayCommand(_ => ExecuteHeadingCommand(1), _ => true);
            Heading2Command = new RelayCommand(_ => ExecuteHeadingCommand(2), _ => true);
            Heading3Command = new RelayCommand(_ => ExecuteHeadingCommand(3), _ => true);
            HorizontalLineCommand = new RelayCommand(_ => ExecuteHorizontalLineCommand(), _ => true);
            UndoCommand = new RelayCommand(_ => ExecuteUndoCommand(), _ => CanUndo);
            RedoCommand = new RelayCommand(_ => ExecuteRedoCommand(), _ => CanRedo);
            CutCommand = new RelayCommand(_ => ExecuteCutCommand(), _ => HasSelection);
            CopyCommand = new RelayCommand(_ => ExecuteCopyCommand(), _ => HasSelection);
            PasteCommand = new RelayCommand(_ => ExecutePasteCommand(), _ => true);

            UpdateCounts();
        }

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
                _innerBox.SelectionChanged += InnerBox_SelectionChanged;
            }
            else
            {
                Debug.WriteLine($"[#{_instanceId}] ERROR: Inner box not found!");
            }
        }

        private void ExecuteUndoCommand() => _innerBox?.Undo();
        private void ExecuteRedoCommand() => _innerBox?.Redo();
        private void ExecuteCutCommand() => _innerBox?.CutSelectionToClipboard();
        private void ExecuteCopyCommand() => _innerBox?.CopySelectionToClipboard();
        private void ExecutePasteCommand() => _innerBox?.PasteFromClipboard();

        private void ExecuteBoldCommand()
        {
            if (_innerBox == null) return;
            ToggleMarkdownFormatting("**");
        }

        private void ExecuteItalicCommand()
        {
            if (_innerBox == null) return;
            ToggleMarkdownFormatting("*");
        }

        private void ExecuteStrikethroughCommand()
        {
            if (_innerBox == null) return;
            ToggleMarkdownFormatting("~~");
        }

        private void ExecuteCodeCommand()
        {
            if (_innerBox == null) return;
            ToggleMarkdownFormatting("`");
        }

        private void ExecuteBulletCommand()
        {
            if (_innerBox == null) return;
            InsertLinePrefix("- ");
        }

        private void ExecuteQuoteCommand()
        {
            if (_innerBox == null) return;
            InsertLinePrefix("> ");
        }

        private void ExecuteHeadingCommand(int level)
        {
            if (_innerBox == null) return;
            string prefix = new string('#', level) + " ";
            InsertLinePrefix(prefix);
        }

        private void ExecuteHorizontalLineCommand()
        {
            if (_innerBox == null) return;
            int start = _innerBox.SelectionStart;
            string text = Text ?? string.Empty;
            string instext = "\n---\n";

            if (start < 0 || start > text.Length)
                start = text.Length;

            text = text.Insert(start, instext);
            Text = text;
            _innerBox.SelectionStart = start + instext.Length;
            _innerBox.SelectionLength = 0;
        }

        /// <summary>
        /// Toggles markdown formatting on the selected text or the current word.
        /// If no selection, applies formatting to the word where cursor is positioned.
        /// Detects existing formatting even if only the content (not markers) is selected.
        /// </summary>
        private void ToggleMarkdownFormatting(string marker)
        {
            if (_innerBox == null) return;

            string text = Text ?? string.Empty;
            int start = _innerBox.SelectionStart;
            int length = _innerBox.SelectionLength;

            if (start < 0 || start > text.Length) start = text.Length;
            if (length < 0 || start + length > text.Length) length = text.Length - start;

            // If no selection, select the current word
            if (length == 0)
            {
                var wordBounds = GetWordBounds(text, start);
                start = wordBounds.start;
                length = wordBounds.length;

                // If cursor is at whitespace/empty, don't format
                if (length == 0)
                    return;
            }

            string selectedText = text.Substring(start, length);

            // Check if content is already formatted (look for markers around selected content)
            var formatInfo = DetectFormattingAround(text, start, length, marker);

            if (formatInfo.isFormatted)
            {
                // Remove formatting - delete markers before and after
                int markerStart = formatInfo.markerStart;
                int markerEnd = formatInfo.markerEnd;
                int contentLength = formatInfo.contentLength;

                // Remove end markers first (to not mess up indices)
                text = text.Remove(markerEnd, marker.Length);
                // Then remove start markers
                text = text.Remove(markerStart, marker.Length);

                Text = text;
                _innerBox.SelectionStart = markerStart;
                _innerBox.SelectionLength = contentLength;
            }
            else
            {
                // Add formatting
                string wrappedText = marker + selectedText + marker;
                text = text.Remove(start, length).Insert(start, wrappedText);

                Text = text;
                _innerBox.SelectionStart = start + marker.Length;
                _innerBox.SelectionLength = length;
            }
        }

        /// <summary>
        /// Gets the bounds of the word at the given position
        /// </summary>
        private (int start, int length) GetWordBounds(string text, int cursorPos)
        {
            if (string.IsNullOrEmpty(text) || cursorPos < 0 || cursorPos > text.Length)
                return (cursorPos, 0);

            // If cursor is at a space or punctuation, return zero length
            if (cursorPos < text.Length && (char.IsWhiteSpace(text[cursorPos]) || char.IsPunctuation(text[cursorPos])))
            {
                // Check if we're between word characters (at a space)
                if (cursorPos > 0 && char.IsLetterOrDigit(text[cursorPos - 1]))
                    return (cursorPos, 0);
            }

            // Find start of word
            int wordStart = cursorPos;
            while (wordStart > 0 && (char.IsLetterOrDigit(text[wordStart - 1]) || text[wordStart - 1] == '_'))
            {
                wordStart--;
            }

            // Find end of word
            int wordEnd = cursorPos;
            while (wordEnd < text.Length && (char.IsLetterOrDigit(text[wordEnd]) || text[wordEnd] == '_'))
            {
                wordEnd++;
            }

            return (wordStart, wordEnd - wordStart);
        }

        /// <summary>
        /// Detects if content is already wrapped with the given markdown marker
        /// </summary>
        private (bool isFormatted, int markerStart, int markerEnd, int contentLength) DetectFormattingAround(string text, int contentStart, int contentLength, string marker)
        {
            // Check if there are markers before and after the content
            int markerLen = marker.Length;

            // Check before
            bool hasMarkerBefore = contentStart >= markerLen &&
                                   text.Substring(contentStart - markerLen, markerLen) == marker;

            // Check after
            bool hasMarkerAfter = contentStart + contentLength + markerLen <= text.Length &&
                                  text.Substring(contentStart + contentLength, markerLen) == marker;

            if (hasMarkerBefore && hasMarkerAfter)
            {
                return (true, contentStart - markerLen, contentStart + contentLength, contentLength);
            }

            // Also check if selection already includes the markers
            if (contentLength >= 2 * markerLen)
            {
                string selectedText = text.Substring(contentStart, contentLength);
                if (selectedText.StartsWith(marker) && selectedText.EndsWith(marker))
                {
                    return (true, contentStart, contentStart + contentLength, contentLength);
                }
            }

            return (false, 0, 0, 0);
        }

        /// <summary>
        /// Adds a prefix to the beginning of the current line(s)
        /// </summary>
        private void InsertLinePrefix(string prefix)
        {
            if (_innerBox == null) return;

            string text = Text ?? string.Empty;
            int start = _innerBox.SelectionStart;
            int selectionLength = _innerBox.SelectionLength;

            if (start < 0) start = 0;
            if (start > text.Length) start = text.Length;
            if (selectionLength < 0) selectionLength = 0;
            if (start + selectionLength > text.Length) selectionLength = text.Length - start;

            // Find line start
            int lineStart = start;
            while (lineStart > 0 && text[lineStart - 1] != '\n')
            {
                lineStart--;
            }

            // Find line end
            int lineEnd = start + selectionLength;
            while (lineEnd < text.Length && text[lineEnd] != '\n')
            {
                lineEnd++;
            }

            // If there's a selection spanning multiple lines, process all lines
            if (selectionLength > 0 && text.Substring(start, selectionLength).Contains('\n'))
            {
                lineStart = start;
                while (lineStart > 0 && text[lineStart - 1] != '\n')
                {
                    lineStart--;
                }

                lineEnd = start + selectionLength;
                if (lineEnd < text.Length && text[lineEnd] == '\n')
                    lineEnd++;

                string lineContent = text.Substring(lineStart, lineEnd - lineStart);
                string[] lines = lineContent.Split('\n');

                string processedLines = string.Join("\n",
                    lines.Select(line => ProcessLinePrefix(line, prefix))
                );

                text = text.Remove(lineStart, lineEnd - lineStart).Insert(lineStart, processedLines);
                Text = text;
                _innerBox.SelectionStart = lineStart;
                _innerBox.SelectionLength = processedLines.Length;
            }
            else
            {
                // Single line - check if prefix already exists
                string lineContent = text.Substring(lineStart, lineEnd - lineStart);
                string newLine = ProcessLinePrefix(lineContent, prefix);

                if (newLine != lineContent)
                {
                    text = text.Remove(lineStart, lineEnd - lineStart).Insert(lineStart, newLine);
                    Text = text;
                    _innerBox.SelectionStart = lineStart + (newLine.StartsWith(prefix) ? prefix.Length : 0);
                    _innerBox.SelectionLength = newLine.Length - (newLine.StartsWith(prefix) ? prefix.Length : 0);
                }
            }
        }

        /// <summary>
        /// Processes a single line to add/remove/replace prefix
        /// For headings, removes old heading level before adding new one
        /// </summary>
        private string ProcessLinePrefix(string line, string prefix)
        {
            if (string.IsNullOrWhiteSpace(line))
                return line;

            // Check if this is a heading prefix (# followed by space)
            bool isHeadingPrefix = prefix.StartsWith("#") && prefix.EndsWith(" ");
            
            if (isHeadingPrefix)
            {
                // Remove any existing heading prefix first
                string trimmedLine = line.TrimStart();
                int hashCount = 0;
                while (hashCount < trimmedLine.Length && trimmedLine[hashCount] == '#')
                {
                    hashCount++;
                }

                // If line starts with #'s followed by space, it's a heading
                if (hashCount > 0 && hashCount < trimmedLine.Length && trimmedLine[hashCount] == ' ')
                {
                    // Remove the old heading
                    trimmedLine = trimmedLine.Substring(hashCount + 1);
                    
                    // Check if new prefix is same level as old - toggle off
                    if (prefix == new string('#', hashCount) + " ")
                    {
                        return trimmedLine;
                    }
                    
                    // Replace with new heading level
                    return prefix + trimmedLine;
                }

                // Line doesn't have a heading yet - add it
                return prefix + line;
            }

            // Non-heading prefix (bullet, quote)
            if (line.StartsWith(prefix))
            {
                // Remove prefix (toggle off)
                return line.Substring(prefix.Length);
            }
            else
            {
                // Add prefix
                return prefix + line;
            }
        }

        private void InnerBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCounts();
            UpdateUndoRedoState();

            if (!_isUpdatingText && !string.IsNullOrEmpty(NoteID))
            {
                _ = NoteFileHandlingHelper.WriteNoteFileAsync(NoteID, Text);
            }
        }

        private void InnerBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_innerBox == null) return;

            bool newHasSelection = _innerBox.SelectionLength > 0;
            
            // Only update if selection state changed
            if (newHasSelection != HasSelection)
            {
                HasSelection = newHasSelection;
            }

            // Update Cut/Copy availability based on selection
            CutCommand?.RaiseCanExecuteChanged();
            CopyCommand?.RaiseCanExecuteChanged();
        }

        private void UpdateUndoRedoState()
        {
            if (_innerBox == null) return;
            CanUndo = _innerBox.CanUndo;
            CanRedo = _innerBox.CanRedo;
        }

        public void InsertLink(string url, string displayText = null)
        {
            if (_innerBox == null)
                return;

            string text = Text ?? string.Empty;

            int start = _innerBox.SelectionStart;
            if (start < 0 || start > text.Length)
                start = text.Length;

            int length = _innerBox.SelectionLength;
            if (length < 0 || start + length > text.Length)
                length = 0;

            if (string.IsNullOrEmpty(displayText))
                displayText = url;

            string linkText = $"[{displayText}]({url})";

            text = text.Remove(start, length).Insert(start, linkText);

            Text = text;
            _innerBox.SelectionStart = start + linkText.Length;
            _innerBox.SelectionLength = 0;
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

        public string Text
        {
            get => (string)(GetValue(TextProperty) ?? string.Empty);
            set
            {
                _isUpdatingText = true;
                SetValue(TextProperty, value ?? string.Empty);
                UpdateCounts();
                _isUpdatingText = false;
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
            string instext = $"""

<details>
<summary>
{summary}
</summary>
{details}
</details>

""";

            if (_innerBox == null)
                return;

            string text = Text ?? string.Empty;

            int start = _innerBox.SelectionStart;
            if (start < 0 || start > text.Length)
                start = text.Length;

            int length = _innerBox.SelectionLength;
            if (length < 0 || start + length > text.Length)
                length = 0;

            text = text.Remove(start, length).Insert(start, instext);

            Text = text;
            _innerBox.SelectionStart = start + instext.Length;
            _innerBox.SelectionLength = 0;
        }
    }
}
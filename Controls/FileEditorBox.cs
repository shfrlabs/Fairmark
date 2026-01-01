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
        private void ToggleMarkdownFormatting(string marker)
        {
            if (_innerBox == null) return;

            string text = Text ?? string.Empty;
            int start = _innerBox.SelectionStart;
            int length = _innerBox.SelectionLength;

            if (start < 0 || start > text.Length) start = text.Length;
            if (length < 0 || start + length > text.Length) length = text.Length - start;

            if (length == 0)
            {
                var wordBounds = GetWordBounds(text, start);
                start = wordBounds.start;
                length = wordBounds.length;

                if (length == 0)
                    return;
            }

            string selectedText = text.Substring(start, length);

            var formatInfo = DetectFormattingAround(text, start, length, marker);

            if (formatInfo.isFormatted)
            {
                int markerStart = formatInfo.markerStart;
                int markerEnd = formatInfo.markerEnd;
                int contentLength = formatInfo.contentLength;

                text = text.Remove(markerEnd, marker.Length);
                text = text.Remove(markerStart, marker.Length);

                Text = text;
                _innerBox.SelectionStart = markerStart;
                _innerBox.SelectionLength = contentLength;
            }
            else
            {
                string wrappedText = marker + selectedText + marker;
                text = text.Remove(start, length).Insert(start, wrappedText);

                Text = text;
                _innerBox.SelectionStart = start + marker.Length;
                _innerBox.SelectionLength = length;
            }
        }
        private (int start, int length) GetWordBounds(string text, int cursorPos)
        {
            if (string.IsNullOrEmpty(text) || cursorPos < 0 || cursorPos > text.Length)
                return (cursorPos, 0);

            if (cursorPos < text.Length && (char.IsWhiteSpace(text[cursorPos]) || char.IsPunctuation(text[cursorPos])))
            {
                if (cursorPos > 0 && char.IsLetterOrDigit(text[cursorPos - 1]))
                    return (cursorPos, 0);
            }

            int wordStart = cursorPos;
            while (wordStart > 0 && (char.IsLetterOrDigit(text[wordStart - 1]) || text[wordStart - 1] == '_'))
            {
                wordStart--;
            }

            int wordEnd = cursorPos;
            while (wordEnd < text.Length && (char.IsLetterOrDigit(text[wordEnd]) || text[wordEnd] == '_'))
            {
                wordEnd++;
            }

            return (wordStart, wordEnd - wordStart);
        }
        private (bool isFormatted, int markerStart, int markerEnd, int contentLength) DetectFormattingAround(string text, int contentStart, int contentLength, string marker)
        {
            int markerLen = marker.Length;

            bool hasMarkerBefore = contentStart >= markerLen &&
                                   text.Substring(contentStart - markerLen, markerLen) == marker;

            bool hasMarkerAfter = contentStart + contentLength + markerLen <= text.Length &&
                                  text.Substring(contentStart + contentLength, markerLen) == marker;

            if (hasMarkerBefore && hasMarkerAfter)
            {
                return (true, contentStart - markerLen, contentStart + contentLength, contentLength);
            }

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

            int lineStart = start;
            while (lineStart > 0 && text[lineStart - 1] != '\n')
            {
                lineStart--;
            }

            int lineEnd = start + selectionLength;
            while (lineEnd < text.Length && text[lineEnd] != '\n')
            {
                lineEnd++;
            }

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
        private string ProcessLinePrefix(string line, string prefix)
        {
            if (string.IsNullOrWhiteSpace(line))
                return line;

            bool isHeadingPrefix = prefix.StartsWith("#") && prefix.EndsWith(" ");
            
            if (isHeadingPrefix)
            {
                string trimmedLine = line.TrimStart();
                int hashCount = 0;
                while (hashCount < trimmedLine.Length && trimmedLine[hashCount] == '#')
                {
                    hashCount++;
                }

                if (hashCount > 0 && hashCount < trimmedLine.Length && trimmedLine[hashCount] == ' ')
                {
                    trimmedLine = trimmedLine.Substring(hashCount + 1);
                    
                    if (prefix == new string('#', hashCount) + " ")
                    {
                        return trimmedLine;
                    }
                    
                    return prefix + trimmedLine;
                }

                return prefix + line;
            }

            if (line.StartsWith(prefix))
            {
                return line.Substring(prefix.Length);
            }
            else
            {
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
            
            if (newHasSelection != HasSelection)
            {
                HasSelection = newHasSelection;
            }

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
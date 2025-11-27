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
            //
        }
        private void ExecuteItalicCommand()
        {
            if (_innerBox == null || _innerBox.SelectionLength == 0) return;
            //
        }
        private void ExecuteStrikethroughCommand()
        {
            if (_innerBox == null || _innerBox.SelectionLength == 0) return;
            //
        }
        private void ExecuteCodeCommand()
        {
            if (_innerBox == null || _innerBox.SelectionLength == 0) return;
            //
        }
        private void ExecuteBulletCommand()
        {
            //
        }
        private void ExecuteQuoteCommand()
        {
            //
        }
        private void ExecuteHeadingCommand(int level) {
            //
        }
        private void ExecuteHorizontalLineCommand() {
            //
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

        
        public void InsertLink(string url, string displayText = null) {
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
        public string Text {
            get => (string)(GetValue(TextProperty) ?? string.Empty);
            set {
                SetValue(TextProperty, value ?? string.Empty);
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

        internal void InsertDetails(string summary, string details) {

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
// Replace your entire FileEditorBox.cs with this optimized version
using Fairmark.Helpers;
using System;
using System.Diagnostics;
using System.Text;
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

        #region Dependency Properties
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
        #endregion

        #region Template and Event Handling
        protected override void OnApplyTemplate() {
            Debug.WriteLine($"[#{_instanceId}] Applying template");
            base.OnApplyTemplate();

            if (_innerBox != null) {
                _innerBox.TextChanged -= InnerBox_TextChanged;
                _innerBox.SelectionChanged -= InnerBox_SelectionChanged;
                _innerBox.PointerPressed -= InnerBox_PointerPressed;
                _innerBox.KeyDown -= InnerBox_KeyDown;
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

                // Initialize with current values
                _lastSelectionStart = _innerBox.SelectionStart;
                _lastSelectionLength = _innerBox.SelectionLength;
                UpdateSelectionProperties();
            }
            else {
                Debug.WriteLine($"[#{_instanceId}] ERROR: Inner box not found!");
            }
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
        #endregion

        #region Formatting Detection and Application
        private void UpdateSelectionProperties() {
            if (_innerBox == null || _updatingSelection || _updatingText)
                return;

            try {
                _updatingSelection = true;
                Debug.WriteLine($"[#{_instanceId}] UPDATING SELECTION PROPERTIES");

                // Update selection status
                bool hasSelection = _innerBox.SelectionLength > 0;
                SetValue(HasSelectionProperty, hasSelection);

                // Update text formatting properties
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

                // If cursor is between lines (at a newline position), get the next line
                if (lineText == "" && cursorPosition < _innerBox.Text.Length &&
                    (_innerBox.Text[cursorPosition] == '\n' || _innerBox.Text[cursorPosition] == '\r')) {
                    (lineStart, lineEnd, lineText, lineNumber) = GetCurrentLine(cursorPosition + 1);
                }

                Debug.WriteLine($"[#{_instanceId}] Updating headings: cursor={cursorPosition}, lastLine={_lastCursorLineStart}, newLine={lineStart}");

                // Existing heading detection logic
                int headingLevel = 0;
                if (lineText.StartsWith("### "))
                    headingLevel = 3;
                else if (lineText.StartsWith("## "))
                    headingLevel = 2;
                else if (lineText.StartsWith("# "))
                    headingLevel = 1;

                SetValue(IsHeading1Property, headingLevel == 1);
                SetValue(IsHeading2Property, headingLevel == 2);
                SetValue(IsHeading3Property, headingLevel == 3);

                // Fire event if cursor moved to a new line
                if (lineStart != _lastCursorLineStart) {
                    Debug.WriteLine($"[#{_instanceId}] Line changed: {_lastCursorLineNumber} -> {lineNumber}");

                    // Fire event with BOTH previous and current line numbers
                    CursorLineChanged?.Invoke(this, new CursorLineChangedEventArgs(
                        lineStart,
                        lineEnd,
                        lineText,
                        lineNumber,          // Current line number
                        _lastCursorLineNumber, // Previous line number
                        headingLevel
                    ));

                    // Update tracking variables AFTER firing event
                    _lastCursorLineStart = lineStart;
                    _lastCursorLineNumber = lineNumber;
                }
            }
            finally {
                _updatingHeadings = false;
            }
        }

        private (int start, int end, string text, int number) GetCurrentLine(int position) {
            string text = _innerBox.Text ?? "";
            if (string.IsNullOrEmpty(text))
                return (0, 0, "", 0);

            // Clamp position to valid range
            position = Math.Clamp(position, 0, text.Length);

            int lineStart = position;
            int lineEnd = position;

            // Find start of line by searching backwards for newline
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

            // Find end of line by searching forwards for newline
            for (int i = position; i <= text.Length; i++) {
                if (i == text.Length || text[i] == '\n' || text[i] == '\r') {
                    lineEnd = i;
                    break;
                }
            }

            // Count ALL lines from start of text to lineStart
            int lineNumber = 1;
            for (int i = 0; i < lineStart; i++) {
                if (text[i] == '\n' || text[i] == '\r') {
                    lineNumber++;
                }
            }

            // Extract line text
            int length = lineEnd - lineStart;
            string lineText = length > 0 ? text.Substring(lineStart, length) : "";

            Debug.WriteLine($"[#{_instanceId}] GetCurrentLine: pos={position}, start={lineStart}, end={lineEnd}, line={lineNumber}, text='{lineText}'");
            return (lineStart, lineEnd, lineText, lineNumber);
        }
        private bool IsFormattedAtPosition(string pattern, bool isItalic) {
            if (_innerBox == null || string.IsNullOrEmpty(_innerBox.Text))
                return false;

            int pos = _innerBox.SelectionStart;
            string text = _innerBox.Text;
            int patternLength = pattern.Length;

            // Handle special case for italic inside bold
            if (isItalic) {
                // Check if we're inside bold formatting
                if (pos > 0 && pos < text.Length - 1) {
                    // If we're between two asterisks that form bold formatting
                    if (text[pos - 1] == '*' && text[pos] == '*') {
                        return false; // It's bold, not italic
                    }
                }
            }

            int openIndex = -1;
            int closeIndex = -1;

            // Find opening pattern (search backwards from cursor)
            for (int i = Math.Min(pos, text.Length - patternLength); i >= 0; i--) {
                if (i + patternLength <= text.Length &&
                    text.Substring(i, patternLength) == pattern) {
                    // Skip if this is part of a longer pattern (like ** for bold)
                    if (isItalic && pattern == "*" &&
                        i < text.Length - 1 && text[i + 1] == '*') {
                        continue;
                    }
                    openIndex = i;
                    break;
                }
            }

            if (openIndex == -1)
                return false;

            // Find closing pattern (search forward from cursor)
            for (int i = Math.Max(openIndex + patternLength, pos); i <= text.Length - patternLength; i++) {
                if (text.Substring(i, patternLength) == pattern) {
                    // Skip if this is part of a longer pattern
                    if (isItalic && pattern == "*" &&
                        i > 0 && text[i - 1] == '*') {
                        continue;
                    }
                    closeIndex = i;
                    break;
                }
            }

            if (closeIndex == -1)
                return false;

            // For cursor position
            if (_innerBox.SelectionLength == 0) {
                // Cursor must be strictly between the markers
                return (openIndex + patternLength < closeIndex) &&
                       (pos > openIndex + patternLength - 1) &&
                       (pos < closeIndex);
            }
            // For selection
            else {
                int start = _innerBox.SelectionStart;
                int end = start + _innerBox.SelectionLength;
                return (openIndex + patternLength <= start) &&
                       (closeIndex >= end);
            }
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

        private static void OnHeadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            Debug.WriteLine("Heading property changed");
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

                // Get current line and heading level
                int cursorPosition = _innerBox.SelectionStart;
                (int lineStart, int lineEnd, string lineText, int lineNumber) = GetCurrentLine(cursorPosition);

                int currentLevel = 0;
                if (lineText.StartsWith("#")) {
                    int i = 0;
                    while (i < lineText.Length && lineText[i] == '#')
                        i++;

                    if (i < lineText.Length && lineText[i] == ' ')
                        currentLevel = i;
                }

                string newLineText;
                int headingPrefixLength = 0;

                if (currentLevel == targetLevel) {
                    // Remove heading if clicking the same level again
                    newLineText = lineText.Substring(currentLevel + 1);
                }
                else if (currentLevel > 0) {
                    // Replace existing heading
                    newLineText = new string('#', targetLevel) + " " + lineText.Substring(currentLevel + 1);
                    headingPrefixLength = targetLevel + 1;
                }
                else {
                    // Add new heading
                    newLineText = new string('#', targetLevel) + " " + lineText;
                    headingPrefixLength = targetLevel + 1;
                }

                // Calculate new text
                string newText = _innerBox.Text.Substring(0, lineStart) +
                                newLineText +
                                _innerBox.Text.Substring(lineEnd);

                // Update text and adjust cursor position
                Text = newText;

                // Position cursor after heading prefix
                int newCursorPosition = lineStart + headingPrefixLength;
                _innerBox.SelectionStart = newCursorPosition;
                _innerBox.SelectionLength = 0;
            }
            finally {
                _updatingText = false;
                UpdateHeadingProperties();
            }
        }

        private void ToggleFormatting(string pattern, bool isItalic) {
            if (_innerBox == null)
                return;

            int start = _innerBox.SelectionStart;
            int length = _innerBox.SelectionLength;
            string text = _innerBox.Text;
            string selectedText = length > 0 ? text.Substring(start, length) : "";
            int patternLength = pattern.Length;
            bool isCurrentlyFormatted = IsFormattedAtPosition(pattern, isItalic);

            try {
                _updatingText = true;
                string newText = text;
                int newStart = start;
                int newLength = length;

                if (isCurrentlyFormatted) {
                    // Remove formatting
                    int openIndex = -1;
                    int closeIndex = -1;

                    // Find opening pattern
                    for (int i = Math.Min(start, text.Length - patternLength); i >= 0; i--) {
                        if (i + patternLength <= text.Length && text.Substring(i, patternLength) == pattern) {
                            // Skip if part of longer pattern (bold markers)
                            if (isItalic && pattern == "*" && i < text.Length - 1 && text[i + 1] == '*')
                                continue;

                            openIndex = i;
                            break;
                        }
                    }

                    // Find closing pattern
                    if (openIndex != -1) {
                        for (int i = Math.Max(openIndex + patternLength, start + length); i <= text.Length - patternLength; i++) {
                            if (i + patternLength <= text.Length && text.Substring(i, patternLength) == pattern) {
                                // Skip if part of longer pattern
                                if (isItalic && pattern == "*" && i > 0 && text[i - 1] == '*')
                                    continue;

                                closeIndex = i;
                                break;
                            }
                        }
                    }

                    if (openIndex != -1 && closeIndex != -1) {
                        newText = text.Remove(closeIndex, patternLength)
                                     .Remove(openIndex, patternLength);
                        newStart = openIndex;
                        newLength = (closeIndex - openIndex - patternLength);
                    }
                    else {
                        return; // Abort if markers not found
                    }
                }
                else {
                    // Add formatting
                    newText = text.Substring(0, start) +
                               pattern +
                               selectedText +
                               pattern +
                               text.Substring(start + length);
                    newStart = start + patternLength;
                    newLength = length;
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
        #endregion

        #region Property Accessors
        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = (FileEditorBox)d;
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
        #endregion
    }
}
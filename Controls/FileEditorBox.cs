using Fairmark.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Fairmark.Controls {
    public sealed class FileEditorBox : Control {
        private TextBox _innerBox;

        public FileEditorBox() {
            this.DefaultStyleKey = typeof(FileEditorBox);
        }

        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(FileEditorBox),
        new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty NoteIDProperty =
            DependencyProperty.Register("NoteID", typeof(string), typeof(FileEditorBox),
                new PropertyMetadata(string.Empty));

        protected override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _innerBox = GetTemplateChild("MarkEditor") as TextBox;
            if (_innerBox != null) {
                var binding = new Binding {
                    Source = this,
                    Path = new PropertyPath(nameof(Text)),
                    Mode = BindingMode.TwoWay
                };
                _innerBox.SetBinding(TextBox.TextProperty, binding);

                _innerBox.TextChanged += async (s, e) =>
                {
                    if (Text != _innerBox.Text)
                        Text = _innerBox.Text;
                    if (!string.IsNullOrEmpty(NoteID)) {
                        await NoteFileHandlingHelper.WriteNoteFileAsync(NoteID, Text);
                    }
                };
            }
        }


        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string NoteID {
            get => (string)GetValue(NoteIDProperty);
            set => SetValue(NoteIDProperty, value);
        }
    }
}
using Fairmark.Helpers;
using Fairmark.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fairmark
{
    public sealed partial class FileEditorPage : Page
    {
        public FileEditorPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            placeholder.Text += JsonSerializer.Serialize(e.Parameter, new JsonSerializerOptions { WriteIndented = true });
            placeholder.Text += "\n Does file exist?" + await NoteFileHandlingHelper.NoteExists((e.Parameter as NoteMetadata).Id); 
        }
    }
}

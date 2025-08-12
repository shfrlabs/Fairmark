using Fairmark.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Fairmark.SettingsPages
{
    public sealed partial class UpgradePage : Page
    {
        public string PlusPrice = "...";
        public string PrevPrice = "...";
        public UpgradePage()
        {
            this.InitializeComponent();
            PlusPrice = "$1.99";
            PrevPrice = "$4.99";
        }
    }
}

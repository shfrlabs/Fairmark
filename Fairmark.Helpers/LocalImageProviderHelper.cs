using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Fairmark.Helpers
{
    public class LocalImageProviderHelper : IImageProvider
    {
        private ImageFolderHelper helper = new ImageFolderHelper();
        public async Task<Image> GetImage(string url)
        {
            Image image = new Image();
            if (ShouldUseThisProvider(url))
            {
                Debug.WriteLine($"LocalImageProviderHelper: Loading image from {url}");
                string imageName = Uri.UnescapeDataString(url.Substring("local:///".Length));
                Debug.WriteLine($"LocalImageProviderHelper: Loading {imageName}");
                if ((await helper.GetImageList()).Any(t => t.Name == imageName))
                {
                    Debug.WriteLine($"LocalImageProviderHelper: Found image {imageName}");
                    image.Source = new BitmapImage(new Uri($"ms-appdata:///local/default/Images/{Uri.EscapeDataString(imageName)}"));
                    image.Loaded += (s, e) =>
                    {
                        Debug.WriteLine($"LocalImageProviderHelper: Image {imageName} loaded successfully");
                    };
                }
                else
                {
                    Debug.WriteLine($"LocalImageProviderHelper: Image {imageName} not found");
                }
                return image;
            }
            else
            {
                return new Image();
            }
        }

        public bool ShouldUseThisProvider(string url)
        {
            if (url.StartsWith("local://"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

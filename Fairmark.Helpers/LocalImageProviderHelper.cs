using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Fairmark.Helpers {
    public class LocalImageProviderHelper : IImageProvider {
        private readonly ImageFolderHelper helper = new ImageFolderHelper();

        private readonly Dictionary<string, ImageSource> _cache = new Dictionary<string, ImageSource>(StringComparer.Ordinal);

        public async Task<Image> GetImage(string url) {
            if (!ShouldUseThisProvider(url))
                return new Image();

            string imageName = Uri.UnescapeDataString(url.Substring("local:///".Length));
            Debug.WriteLine($"LocalImageProviderHelper: Resolving {imageName}");

            if (!_cache.TryGetValue(imageName, out var cachedSource)) {
                if ((await helper.GetImageList()).Any(t => t.Name == imageName)) {
                    var bitmap = new BitmapImage(new Uri(
                        $"ms-appdata:///local/default/Images/{Uri.EscapeDataString(imageName)}"
                    ));

                    _cache[imageName] = bitmap;
                    cachedSource = bitmap;

                    Debug.WriteLine($"LocalImageProviderHelper: Cached {imageName}");
                }
                else {
                    Debug.WriteLine($"LocalImageProviderHelper: Image {imageName} not found");
                }
            }

            var image = new Image {
                Source = cachedSource,
                Stretch = Stretch.Uniform
            };

            return image;
        }

        public bool ShouldUseThisProvider(string url) {
            return url.StartsWith("local://", StringComparison.OrdinalIgnoreCase);
        }
    }
}
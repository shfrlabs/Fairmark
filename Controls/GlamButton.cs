using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Color = Windows.UI.Color;

namespace Fairmark.Controls {
    public sealed class GlamButton : Button {
        private CanvasControl _glowCanvas;
        private Run _priceRun;
        private Run _prevRun;
        private Run _upgradeRun;
        private TextBlock _contentText;
        private bool _isPointerOver;

        private readonly ResourceLoader _resLoader =
            ResourceLoader.GetForCurrentView();

        public string PlusPrice {
            get => (string)GetValue(PlusPriceProperty);
            set => SetValue(PlusPriceProperty, value);
        }

        public string PrevPrice {
            get => (string)GetValue(PrevPriceProperty);
            set => SetValue(PrevPriceProperty, value);
        }

        public bool IsOnPromotion {
            get => (bool)GetValue(IsOnPromotionProperty);
            set => SetValue(IsOnPromotionProperty, value);
        }

        public static readonly DependencyProperty PlusPriceProperty =
            DependencyProperty.Register(nameof(PlusPrice), typeof(string),
                typeof(GlamButton), new PropertyMetadata(string.Empty, OnPlusPriceChanged));

        public static readonly DependencyProperty PrevPriceProperty =
            DependencyProperty.Register(nameof(PrevPrice), typeof(string),
                typeof(GlamButton), new PropertyMetadata(string.Empty, OnPrevPriceChanged));

        public static readonly DependencyProperty IsOnPromotionProperty =
            DependencyProperty.Register(nameof(IsOnPromotion), typeof(bool),
                typeof(GlamButton), new PropertyMetadata(false, OnIsOnPromotionChanged));

        private static void OnPlusPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is GlamButton button)
                button.RebuildInlines();
        }

        private static void OnPrevPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is GlamButton button)
                button.RebuildInlines();
        }

        private static void OnIsOnPromotionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is GlamButton button)
                button.RebuildInlines();
        }

        public GlamButton() {
            DefaultStyleKey = typeof(GlamButton);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected override void OnApplyTemplate() {
            base.OnApplyTemplate();
            _glowCanvas = GetTemplateChild("GlowCanvas") as CanvasControl;
            _contentText = GetTemplateChild("ContentText") as TextBlock;

            if (_glowCanvas != null) {
                _glowCanvas.Draw += OnGlowDraw;
                _glowCanvas.CreateResources += OnGlowCreateResources;
            }

            RebuildInlines();
        }

        private void RebuildInlines() {
            if (_contentText == null)
                return;

            _contentText.Inlines.Clear();

            string upgradeText = _resLoader.GetString("UpgradeFor");
            _upgradeRun = new Run { Text = upgradeText + " " };
            _contentText.Inlines.Add(_upgradeRun);


            if (IsOnPromotion) {
                _prevRun = new Run {
                    Text = PrevPrice,
                    TextDecorations = Windows.UI.Text.TextDecorations.Strikethrough
                };
                _contentText.Inlines.Add(_prevRun);
            }

            _priceRun = new Run {
                Text = IsOnPromotion ? " " + PlusPrice : PlusPrice,
                FontWeight = Windows.UI.Text.FontWeights.Bold
            };
            _contentText.Inlines.Add(_priceRun);
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            PointerEntered += (s, args) => {
                _isPointerOver = true;
                _glowCanvas?.Invalidate();
            };

            PointerExited += (s, args) => {
                _isPointerOver = false;
                _glowCanvas?.Invalidate();
            };
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            if (_glowCanvas != null) {
                _glowCanvas.Draw -= OnGlowDraw;
                _glowCanvas.CreateResources -= OnGlowCreateResources;
                _glowCanvas.RemoveFromVisualTree();
                _glowCanvas = null;
            }
        }

        private void OnGlowCreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args) { }

        private void OnGlowDraw(CanvasControl sender, CanvasDrawEventArgs args) {
            if (!_isPointerOver) {
                args.DrawingSession.Clear(Colors.Transparent);
                return;
            }

            using (var glow = new CanvasCommandList(sender)) {
                using (var clds = glow.CreateDrawingSession()) {
                    var gradientStops = new[] {
                        new CanvasGradientStop { Color = Color.FromArgb(60, 0xAF, 0x72, 0xD3), Position = 0f },
                        new CanvasGradientStop { Color = Color.FromArgb(30, 0x8F, 0xDD, 0xF1), Position = 1f }
                    };

                    var gradient = new CanvasLinearGradientBrush(sender, gradientStops) {
                        StartPoint = new System.Numerics.Vector2(0, 0),
                        EndPoint = new System.Numerics.Vector2(0, 1)
                    };

                    float margin = 50f;
                    float width = (float)sender.ActualWidth + margin * 2;
                    float height = (float)sender.ActualHeight + margin * 2;

                    clds.FillRectangle(-margin, -margin, width, height, gradient);
                }

                var blur = new GaussianBlurEffect {
                    Source = glow,
                    BlurAmount = 25f,
                    BorderMode = EffectBorderMode.Soft,
                    Optimization = EffectOptimization.Balanced
                };

                args.DrawingSession.DrawImage(blur, 0, 0, new Rect(0, 0, sender.Width, sender.Height), 0.6f);
            }
        }
    }
}

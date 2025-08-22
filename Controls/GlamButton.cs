using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Drawing;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Color = Windows.UI.Color;

namespace Fairmark.Controls {
    public sealed class GlamButton : Button {
        private CanvasControl _glowCanvas;
        private Run _priceRun;
        private Run _prevRun;
        private bool _isPointerOver;
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
            if (d is GlamButton button && button._priceRun != null)
                button._priceRun.Text = e.NewValue?.ToString();
        }
        private static void OnPrevPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is GlamButton button && button._prevRun != null) {
                button._prevRun.Text = e.NewValue?.ToString();
                button.UpdatePrevPriceVisibility();
            }
        }

        private static void OnIsOnPromotionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is GlamButton button) {
                button.UpdatePrevPriceVisibility();
            }
        }

        private void UpdatePrevPriceVisibility() {
            if (_prevRun != null) {
                _prevRun.Text = IsOnPromotion ? PrevPrice : null;
            }
        }

        public GlamButton() {
            DefaultStyleKey = typeof(GlamButton);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected override void OnApplyTemplate() {
            base.OnApplyTemplate();
            _glowCanvas = GetTemplateChild("GlowCanvas") as CanvasControl;
            _priceRun = GetTemplateChild("PriceRun") as Run;
            _prevRun = GetTemplateChild("PrevPriceRun") as Run;

            if (_priceRun != null)
                _priceRun.Text = PlusPrice;

            if (_prevRun != null) {
                _prevRun.Text = PrevPrice;
                UpdatePrevPriceVisibility();
            }

            if (_glowCanvas != null) {
                _glowCanvas.Draw += OnGlowDraw;
                _glowCanvas.CreateResources += OnGlowCreateResources;
            }
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

        private void OnGlowCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args) {
        }

        private void OnGlowDraw(CanvasControl sender, CanvasDrawEventArgs args) {
            if (!_isPointerOver) {
                args.DrawingSession.Clear(Colors.Transparent);
                return;
            }

            using (var glow = new CanvasCommandList(sender)) {
                using (var clds = glow.CreateDrawingSession()) {
                    // Create gradient stops with reduced opacity
                    var gradientStops = new[]
                    {
                new CanvasGradientStop { Color = Color.FromArgb(60, 0xAF, 0x72, 0xD3), Position = 0f },
                new CanvasGradientStop { Color = Color.FromArgb(30, 0x8F, 0xDD, 0xF1), Position = 1f }
            };

                    // Create gradient brush (using correct constructor)
                    var gradient = new CanvasLinearGradientBrush(sender, gradientStops) {
                        StartPoint = new System.Numerics.Vector2(0, 0),
                        EndPoint = new System.Numerics.Vector2(0, 1),
                        // Edge behavior is controlled by the command list size
                    };

                    // Draw larger than the button to allow blur to extend
                    float margin = 50f;
                    float width = (float)sender.ActualWidth + margin * 2;
                    float height = (float)sender.ActualHeight + margin * 2;

                    clds.FillRectangle(-margin, -margin, width, height, gradient);
                }

                // Apply blur effect
                var blur = new GaussianBlurEffect {
                    Source = glow,
                    BlurAmount = 25f,  // Reduced for subtlety
                    BorderMode = EffectBorderMode.Soft,
                    Optimization = EffectOptimization.Balanced
                };

                // Draw with additional transparency
                args.DrawingSession.DrawImage(blur, 0, 0, new Rect(0, 0, sender.Width, sender.Height), 0.6f);
            }
        }
    }
}
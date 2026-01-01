using Fairmark.Helpers;
using Fairmark.Intelligence;
using Fairmark.Intelligence.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Fairmark.SettingsPages
{
    public sealed partial class AIPage : Page, INotifyPropertyChanged
    {
        private CancellationTokenSource _currentCancellationTokenSource;
        private readonly Windows.ApplicationModel.Resources.ResourceLoader _loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
        public event PropertyChangedEventHandler PropertyChanged;
        public AIPage()
        {
            this.InitializeComponent();
            (Application.Current.Resources["Settings"] as Settings)?.ThemeSettingChanged += (s, e) =>
            {
                if (Window.Current.Content is Frame frame)
                {
                    frame.RequestedTheme = e.Theme;
                }
            };
            if (ais != null)
            {
                ais.PropertyChanged += Ais_PropertyChanged;
                ais.RefreshModels();
            }
        }

        private void Ais_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AISettings.SelectedProvider))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentProvider)));
                Debug.WriteLine($"[AIPage] Provider changed. UI updated to: {currentProvider?.Name}");
            }
        }

        public AISettings ais => App.AISettings;
        public ILLMProvider currentProvider
        {
            get
            {
                var providerType = ais.ProviderByName(ais.SelectedProvider);
                return providerType != null ? Activator.CreateInstance(providerType) as ILLMProvider : null;
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[PollinationsDebug] Send_Click triggered.");

            var textBox = this.FindName("TextBox") as TextBox ?? FindTextBoxInPage();
            var responseBlock = this.FindName("Response") as TextBlock;

            if (textBox == null || responseBlock == null) return;

            string userInput = textBox.Text?.Trim();
            if (string.IsNullOrEmpty(userInput)) return;

            Send.IsEnabled = false;
            responseBlock.Text = _loader.GetString("AIPage_Status_Processing");

            _currentCancellationTokenSource?.Cancel();
            _currentCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var selectedModel = ais.SelectedModel;
                var selectedProviderName = ais.SelectedProvider;

                var providerType = ais.ProviderByName(selectedProviderName);
                if (providerType == null || !(Activator.CreateInstance(providerType) is ILLMProvider provider))
                {
                    responseBlock.Text = _loader.GetString("AIPage_Error_Instantiation");
                    return;
                }

                Debug.WriteLine($"[AIPage] Sending with Provider: {provider.Name}");
                Debug.WriteLine($"[AIPage] API Key Length: {(provider.ApiKey?.Length ?? 0)}");

                var chatHistory = new List<LLMChatMessage>
                {
                    new LLMChatMessage { Role = LLMChatRole.User, Content = userInput }
                };

                await StreamResponseAsync(provider, chatHistory, selectedModel?.Name, responseBlock, _currentCancellationTokenSource.Token);

                if (!_currentCancellationTokenSource.Token.IsCancellationRequested)
                {
                }
            }
            catch (Exception ex)
            {
                responseBlock.Text = string.Format(_loader.GetString("AIPage_Error_Generic"), ex.Message);
            }
            finally
            {
                Send.IsEnabled = true;
            }
        }

        private async Task StreamResponseAsync(ILLMProvider provider, List<LLMChatMessage> chatHistory, string modelName, TextBlock responseBlock, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                await foreach (var streamed in provider.StreamChat(chatHistory, modelName, cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        responseBlock.Text += streamed.ContentPart;
                    }).AsTask().Wait();
                }
            }, cancellationToken);
        }

        private TextBox FindTextBoxInPage()
        {
            if (this.Content is ScrollViewer scrollViewer &&
                scrollViewer.Content is StackPanel mainStack)
            {
                foreach (var child in mainStack.Children)
                {
                    if (child is StackPanel innerStack)
                    {
                        foreach (var innerChild in innerStack.Children)
                        {
                            if (innerChild is TextBox textBox)
                                return textBox;
                        }
                    }
                }
            }
            return null;
        }
    }
}

using Fairmark.Helpers;
using Fairmark.Intelligence;
using Fairmark.Intelligence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Fairmark.SettingsPages
{
    public sealed partial class AIPage : Page
    {
        private CancellationTokenSource _currentCancellationTokenSource;
        // Add this field to your AIPage class
        private readonly Windows.ApplicationModel.Resources.ResourceLoader _loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
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
            ais?.RefreshModels();
        }

        public AISettings ais => App.AISettings;
        public ILLMProvider currentProvider => ais.ProviderByName(ais.SelectedProvider) is Type providerType
            ? Activator.CreateInstance(providerType) as ILLMProvider
            : null;

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            var textBox = this.FindName("TextBox") as TextBox ?? FindTextBoxInPage();
            var responseBlock = this.FindName("Response") as TextBlock;
            
            if (textBox == null || responseBlock == null)
                return;

            string userInput = textBox.Text?.Trim();
            if (string.IsNullOrEmpty(userInput))
                return;

            Send.IsEnabled = false;
            responseBlock.Text = _loader.GetString("AIPage_Status_Processing"); //

            _currentCancellationTokenSource?.Cancel();
            _currentCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var selectedModel = ais.SelectedModel;
                var selectedProvider = ais.SelectedProvider;

                if (string.IsNullOrEmpty(selectedProvider) || selectedModel == null)
                {
                    responseBlock.Text = _loader.GetString("AIPage_Error_NoSelection"); //
                    return;
                }

                var providerType = ais.ProviderByName(selectedProvider);
                if (providerType == null || !(Activator.CreateInstance(providerType) is ILLMProvider provider))
                {
                    responseBlock.Text = _loader.GetString("AIPage_Error_Instantiation"); //
                    return;
                }

                // ... chat logic and streaming ...

                if (!_currentCancellationTokenSource.Token.IsCancellationRequested)
                {
                    responseBlock.Text += _loader.GetString("AIPage_Status_Completed"); //
                }
            }
            catch (OperationCanceledException)
            {
                responseBlock.Text += _loader.GetString("AIPage_Status_Cancelled"); //
            }
            catch (Exception ex)
            {
                // Use string.Format to inject the exception message into the localized template
                responseBlock.Text = string.Format(_loader.GetString("AIPage_Error_Generic"), ex.Message); //
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

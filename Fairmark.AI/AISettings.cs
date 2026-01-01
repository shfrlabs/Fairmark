using Fairmark.Intelligence.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.Intelligence
{
    public class AISettings : INotifyPropertyChanged
    {
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
        private const string ProviderKey = "aiProvider";
        private const string ModelKey = "selectedModel";
        private const string AIEnabledKey = "fairmarkAI";

        public event PropertyChangedEventHandler PropertyChanged;

        private LLMModelInfo[] _availableModels = Array.Empty<LLMModelInfo>();

        public AISettings()
        {
            _ = UpdateAvailableModelsAsync();
        }

        public string[] AvailableProviders
        {
            get
            {
                var providerAssembly = typeof(ILLMProvider).Assembly;
                return providerAssembly.GetTypes()
                    .Where(t => typeof(ILLMProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .Select(t => {
                        try
                        {
                            return (Activator.CreateInstance(t) as ILLMProvider)?.Name;
                        }
                        catch { return null; }
                    })
                    .Where(name => name != null)
                    .ToArray();
            }
        }

        public LLMModelInfo[] AvailableModels
        {
            get => _availableModels;
            private set
            {
                _availableModels = value;
                OnPropertyChanged(nameof(AvailableModels));
                OnPropertyChanged(nameof(SelectedModel));
            }
        }

        private string GetPerProviderModelKey()
        {
            var provider = SelectedProvider;
            if (string.IsNullOrEmpty(provider)) return ModelKey;
            return $"{ModelKey}_{provider}";
        }

        public async Task UpdateAvailableModelsAsync()
        {
            Debug.WriteLine($"[PollinationsDebug] UpdateAvailableModelsAsync for provider: {SelectedProvider}");
            var providerType = ProviderByName(SelectedProvider);

            if (providerType != null && Activator.CreateInstance(providerType) is ILLMProvider provider)
            {
                try
                {
                    Debug.WriteLine($"[PollinationsDebug] Calling GetAvailableModelsAsync on {provider.Name}...");
                    var models = await provider.GetAvailableModelsAsync();

                    var modelArray = models?.ToArray() ?? Array.Empty<LLMModelInfo>();
                    Debug.WriteLine($"[PollinationsDebug] Models loaded: {modelArray.Length}");

                    AvailableModels = modelArray;
                    ValidateSelectedModel();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[PollinationsDebug] Failed to load models for {provider.Name}: {ex}");
                    AvailableModels = Array.Empty<LLMModelInfo>();
                }
            }
            else
            {
                Debug.WriteLine($"[PollinationsDebug] Could not instantiate provider or provider is null.");
            }
        }

        private void ValidateSelectedModel()
        {
            string specificKey = GetPerProviderModelKey();

            var currentModelName = _localSettings.Values.ContainsKey(specificKey)
                ? _localSettings.Values[specificKey]?.ToString()
                : null;

            var match = AvailableModels.FirstOrDefault(m => m.Name == currentModelName);

            if (match != null)
            {
                SelectedModel = match;
            }
            else if (AvailableModels.Length > 0)
            {
                SelectedModel = AvailableModels[0];
            }
        }

        public Type ProviderByName(string name)
        {
            var providerAssembly = typeof(ILLMProvider).Assembly;
            return providerAssembly.GetTypes()
                .Where(t => typeof(ILLMProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .FirstOrDefault(t =>
                {
                    try
                    {
                        return (Activator.CreateInstance(t) as ILLMProvider)?.Name == name;
                    }
                    catch { return false; }
                });
        }

        public string SelectedProvider
        {
            get
            {
                if (_localSettings.Values.TryGetValue(ProviderKey, out object providerObj) && providerObj is string pn)
                {
                    return pn;
                }
                return AvailableProviders.FirstOrDefault() ?? string.Empty;
            }
            set
            {
                if (SelectedProvider != value)
                {
                    _localSettings.Values[ProviderKey] = value;
                    OnPropertyChanged(nameof(SelectedProvider));
                    _ = UpdateAvailableModelsAsync();
                }
            }
        }

        public LLMModelInfo SelectedModel
        {
            get
            {
                string specificKey = GetPerProviderModelKey();

                var modelName = _localSettings.Values.ContainsKey(specificKey)
                    ? _localSettings.Values[specificKey]?.ToString()
                    : null;

                return AvailableModels.FirstOrDefault(m => m.Name == modelName) ?? AvailableModels.FirstOrDefault();
            }
            set
            {
                string specificKey = GetPerProviderModelKey();

                if (value != null)
                {
                    _localSettings.Values[specificKey] = value.Name;
                }
                else
                {
                    _localSettings.Values[specificKey] = string.Empty;
                }
                OnPropertyChanged(nameof(SelectedModel));
            }
        }

        public bool IsAIEnabled
        {
            get {
                //return _localSettings.Values.TryGetValue(AIEnabledKey, out object aiObj) && aiObj is bool b && b;
                return false;
            }
            set
            {
                _localSettings.Values[AIEnabledKey] = value;
                OnPropertyChanged(nameof(IsAIEnabled));
            }
        }

        public void RefreshModels()
        {
            _ = UpdateAvailableModelsAsync();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
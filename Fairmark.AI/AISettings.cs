using Fairmark.Intelligence.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Storage;

namespace Fairmark.Intelligence
{
    public class AISettings : INotifyPropertyChanged
    {
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
        private const string ProviderKey = "aiProvider";
        private const string ModelKey = "selectedModel";
        private const string AIEnabledKey = "fairmarkAI";

        public AISettings()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string[] AvailableProviders
        {
            get
            {
                List<string> providers = new List<string>();
                var providerAssembly = typeof(ILLMProvider).Assembly;
                var providerTypes = providerAssembly.GetTypes()
                    .Where(t => typeof(ILLMProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in providerTypes)
                {
                    if (Activator.CreateInstance(type) is ILLMProvider provider)
                    {
                        providers.Add(provider.Name);
                    }
                }
                return providers.ToArray();
            }
        }

        public LLMModelInfo[] AvailableModels
        {
            get
            {
                var providerType = ProviderByName(SelectedProvider);
                if (providerType != null && Activator.CreateInstance(providerType) is ILLMProvider provider)
                {
                    return provider.GetAvailableModels().ToArray();
                }
                return new LLMModelInfo[0];
            }
        }

        public Type ProviderByName(string name)
        {
            var providerAssembly = typeof(ILLMProvider).Assembly;
            var providerTypes = providerAssembly.GetTypes()
                .Where(t => typeof(ILLMProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            var result = providerTypes.FirstOrDefault(t =>
            {
                if (Activator.CreateInstance(t) is ILLMProvider provider)
                {
                    return provider.Name == name;
                }
                return false;
            });
            return result;
        }

        public string SelectedProvider
        {
            get
            {
                string providerName = null;
                if (_localSettings.Values.TryGetValue(ProviderKey, out object providerObj) && providerObj is string pn && !string.IsNullOrWhiteSpace(pn))
                {
                    if (AvailableProviders.Contains(pn))
                        providerName = pn;
                }
                if (providerName == null)
                    providerName = AvailableProviders.FirstOrDefault() ?? string.Empty;
                return providerName;
            }
            set
            {
                string newProvider = value;
                if (!string.IsNullOrWhiteSpace(newProvider) && AvailableProviders.Contains(newProvider))
                {
                    _localSettings.Values[ProviderKey] = newProvider;
                }
                else if (AvailableProviders.Length > 0)
                {
                    newProvider = AvailableProviders[0];
                    _localSettings.Values[ProviderKey] = newProvider;
                }
                else
                {
                    newProvider = string.Empty;
                    _localSettings.Values[ProviderKey] = newProvider;
                }

                string newModelName = string.Empty;
                if (!string.IsNullOrEmpty(newProvider))
                {
                    var providerType = ProviderByName(newProvider);
                    if (providerType != null && Activator.CreateInstance(providerType) is ILLMProvider provider)
                    {
                        var availableModels = provider.GetAvailableModels();
                        newModelName = availableModels.FirstOrDefault()?.Name ?? string.Empty;
                    }
                }
                _localSettings.Values[ModelKey] = newModelName;
                OnPropertyChanged(nameof(SelectedProvider));
                OnPropertyChanged(nameof(AvailableModels));
                OnPropertyChanged(nameof(SelectedModel));
            }
        }

        public LLMModelInfo SelectedModel
        {
            get
            {
                string providerName = SelectedProvider;
                var availableModels = AvailableModels;
                if (string.IsNullOrEmpty(providerName) || availableModels == null || availableModels.Length == 0)
                    return availableModels?.FirstOrDefault();

                if (_localSettings.Values.TryGetValue(ModelKey, out object modelObj) && modelObj is string modelName && !string.IsNullOrWhiteSpace(modelName))
                {
                    var found = availableModels.FirstOrDefault(m => m.Name == modelName);
                    if (found != null)
                        return found;
                }
                return availableModels.FirstOrDefault();
            }
            set
            {
                var availableModels = AvailableModels;
                if (value == null || availableModels == null || availableModels.Length == 0)
                {
                    var fallback = availableModels?.FirstOrDefault();
                    _localSettings.Values[ModelKey] = fallback?.Name ?? string.Empty;
                    OnPropertyChanged(nameof(SelectedModel));
                    return;
                }
                var match = availableModels.FirstOrDefault(m => m.Name == value.Name);
                if (match != null)
                {
                    _localSettings.Values[ModelKey] = match.Name;
                }
                else
                {
                    var fallback = availableModels.FirstOrDefault();
                    _localSettings.Values[ModelKey] = fallback?.Name ?? string.Empty;
                }
                OnPropertyChanged(nameof(SelectedModel));
            }
        }

        public bool IsAIEnabled
        {
            get
            {
                return _localSettings.Values.TryGetValue(AIEnabledKey, out object aiObj) && aiObj is bool b && b;
            }
            set
            {
                _localSettings.Values[AIEnabledKey] = value;
                OnPropertyChanged(nameof(IsAIEnabled));
            }
        }

        public void RefreshModels()
        {
            OnPropertyChanged(nameof(AvailableModels));
            OnPropertyChanged(nameof(SelectedModel));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
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
            // Opcjonalnie: zainicjuj ładowanie modeli dla domyślnego dostawcy przy starcie
            _ = UpdateAvailableModelsAsync();
        }

        public string[] AvailableProviders
        {
            get
            {
                // Refleksja jest OK, ale warto odfiltrować abstrakcyjne typy
                var providerAssembly = typeof(ILLMProvider).Assembly;
                return providerAssembly.GetTypes()
                    .Where(t => typeof(ILLMProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .Select(t => {
                        try
                        {
                            // Tworzymy instancję tylko by pobrać nazwę
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
                // Powiadamiamy, że SelectedModel mógł się zmienić w wyniku zmiany listy modeli
                OnPropertyChanged(nameof(SelectedModel));
            }
        }

        public async Task UpdateAvailableModelsAsync()
        {
            var providerType = ProviderByName(SelectedProvider);
            if (providerType != null && Activator.CreateInstance(providerType) is ILLMProvider provider)
            {
                try
                {
                    var models = await provider.GetAvailableModelsAsync();
                    AvailableModels = models?.ToArray() ?? Array.Empty<LLMModelInfo>();
                    ValidateSelectedModel();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to load models for {provider.Name}: {ex.Message}");
                    AvailableModels = Array.Empty<LLMModelInfo>();
                }
            }
        }

        private void ValidateSelectedModel()
        {
            var currentModelName = _localSettings.Values.ContainsKey(ModelKey) ? _localSettings.Values[ModelKey]?.ToString() : null;
            var match = AvailableModels.FirstOrDefault(m => m.Name == currentModelName);

            // Jeśli zapamiętany model nie istnieje w nowym dostawcy, wybierz pierwszy dostępny
            if (match == null && AvailableModels.Length > 0)
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

                    // Odśwież modele asynchronicznie - to zaktualizuje AvailableModels i SelectedModel
                    _ = UpdateAvailableModelsAsync();
                }
            }
        }

        public LLMModelInfo SelectedModel
        {
            get
            {
                var modelName = _localSettings.Values.ContainsKey(ModelKey) ? _localSettings.Values[ModelKey]?.ToString() : null;
                return AvailableModels.FirstOrDefault(m => m.Name == modelName) ?? AvailableModels.FirstOrDefault();
            }
            set
            {
                if (value != null)
                {
                    _localSettings.Values[ModelKey] = value.Name;
                }
                else
                {
                    _localSettings.Values[ModelKey] = string.Empty;
                }
                OnPropertyChanged(nameof(SelectedModel));
            }
        }

        public bool IsAIEnabled
        {
            get => _localSettings.Values.TryGetValue(AIEnabledKey, out object aiObj) && aiObj is bool b && b;
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
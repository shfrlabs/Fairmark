using Fairmark.Intelligence;
using Fairmark.Intelligence.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.AI.Providers
{
    public class PollinationsProvider : ILLMProvider
    {
        private const string BaseUrl = "https://text.pollinations.ai";
        private readonly Windows.ApplicationModel.Resources.ResourceLoader _loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
        public string Name => _loader.GetString("Provider_Pollinations");
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public string ApiKey
        {
            get
            {
                string current = _localSettings.Values["pollinationsKey"] as string;
                return current;
            }
            set
            {
                _localSettings.Values["pollinationsKey"] = value;
            }
        }

        private static readonly HttpClient _httpClient = new HttpClient();
        private List<LLMModelInfo> _cachedModels = null;
        private readonly SemaphoreSlim _modelLock = new SemaphoreSlim(1, 1);

        public async Task<IEnumerable<LLMModelInfo>> GetAvailableModelsAsync()
        {
            if (_cachedModels != null) return _cachedModels;

            await _modelLock.WaitAsync();
            try
            {
                if (_cachedModels != null) return _cachedModels;

                var fetchedModels = new List<LLMModelInfo>();

                // Pollinations models endpoint often returns a simple array of strings
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/models");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                using var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Pollinations /models failed: {response.StatusCode}");
                    // Fallback to default if API fails
                    return new[] { new LLMModelInfo { Name = "error", Description = "🔑" } };
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        string modelName = null;
                        string description = null;

                        // HANDLE BOTH FORMATS: ["gpt-4"] OR [{"name": "gpt-4", ...}]
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            modelName = element.GetString();
                            description = modelName;
                        }
                        else if (element.ValueKind == JsonValueKind.Object)
                        {
                            if (element.TryGetProperty("name", out var n)) modelName = n.GetString();
                            if (element.TryGetProperty("description", out var d)) description = d.GetString();
                        }

                        if (!string.IsNullOrEmpty(modelName))
                        {
                            fetchedModels.Add(new LLMModelInfo
                            {
                                Name = modelName,
                                Description = description ?? modelName
                            });
                        }
                    }
                }

                // If list is empty (parsing failed), add default
                if (fetchedModels.Count == 0)
                {
                    fetchedModels.Add(new LLMModelInfo { Name = "openai", Description = "OpenAI" });
                }

                _cachedModels = fetchedModels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Pollinations Model Fetch Error: {ex.Message}");
                return new[] { new LLMModelInfo { Name = "openai", Description = "OpenAI (Fallback)" } };
            }
            finally
            {
                _modelLock.Release();
            }

            return _cachedModels;
        }

        public async IAsyncEnumerable<LLMStreamedNote> StreamChat(IEnumerable<LLMChatMessage> chatHistory, string modelName = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(modelName)) modelName = "openai";

            var requestPayload = new
            {
                model = modelName,
                messages = BuildMessages(chatHistory),
                stream = true
            };

            var jsonPayload = JsonSerializer.Serialize(requestPayload);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/openai/chat/completions");
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            HttpResponseMessage response = null;
            string connectionError = null;

            // 1. Próba wysłania żądania (bez yield w catch)
            try
            {
                response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            }
            catch (Exception ex)
            {
                // Zapisujemy błąd, ale nie robimy tu yield return
                connectionError = $"Failed to connect: {ex.Message}";
            }

            // 2. Obsługa błędu połączenia (poza blokiem catch)
            if (connectionError != null)
            {
                yield return new LLMStreamedNote
                {
                    Title = _loader.GetString("Title_ConnectionError"),
                    ContentPart = string.Format(_loader.GetString("Error_ConnectionFailed"), connectionError)
                };
                yield break;
            }

            // 3. Obsługa błędu HTTP (np. 404, 500)
            if (!response.IsSuccessStatusCode)
            {
                yield return new LLMStreamedNote { Title = "API Error", ContentPart = $"API returned {response.StatusCode}" };
                response.Dispose();
                yield break;
            }

            // 4. Czytanie strumienia
            using (response)
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

                    var data = line.Substring(6).Trim();
                    if (data == "[DONE]") break;

                    string content = null;
                    try
                    {
                        using var doc = JsonDocument.Parse(data);
                        if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var element = choices[0];
                            if (element.TryGetProperty("delta", out var delta))
                            {
                                if (delta.TryGetProperty("content", out var textElement))
                                {
                                    content = textElement.GetString();
                                }
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(content))
                    {
                        yield return new LLMStreamedNote { Title = "Pollinations AI", ContentPart = content };
                    }
                }
            }
        }
        public IAsyncEnumerable<LLMStreamedNote> StreamCreateNote(string promptOrDocument, string modelName = null, CancellationToken cancellationToken = default)
        {
            var chatHistory = new List<LLMChatMessage>
            {
                new LLMChatMessage { Role = LLMChatRole.User, Content = $"{SystemPrompts.NoteCreationPrompt}\n\n{promptOrDocument}" }
            };
            return StreamChat(chatHistory, modelName, cancellationToken);
        }

        public IAsyncEnumerable<LLMStreamedNote> StreamSummarizeNote(string noteContent, string modelName = null, CancellationToken cancellationToken = default)
        {
            var chatHistory = new List<LLMChatMessage>
            {
                new LLMChatMessage { Role = LLMChatRole.User, Content = $"{SystemPrompts.SummarizationPrompt}\n\n{noteContent}" }
            };
            return StreamChat(chatHistory, modelName, cancellationToken);
        }

        private List<object> BuildMessages(IEnumerable<LLMChatMessage> chatHistory)
        {
            return chatHistory.Select(msg => new
            {
                role = msg.Role.ToString().ToLowerInvariant(), // "user" or "assistant"
                content = msg.Content
            }).Cast<object>().ToList();
        }
    }
}
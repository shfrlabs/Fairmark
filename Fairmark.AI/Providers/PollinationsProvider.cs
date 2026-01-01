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
using System.Diagnostics;

namespace Fairmark.AI.Providers
{
    public class PollinationsProvider : ILLMProvider
    {
        private const string BaseUrl = "https://text.pollinations.ai";
        private readonly Windows.ApplicationModel.Resources.ResourceLoader _loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
        public string Name => _loader.GetString("Provider_Pollinations");
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public string LastUsedModel
        {
            get
            {
                string current = _localSettings.Values["pollinationsLastModel"] as string;
                return current;
            }
            set
            {
                _localSettings.Values["pollinationsLastModel"] = value?.Trim();
            }
        }
        public string ApiKey
        {
            get
            {
                string current = _localSettings.Values["pollinationsKey"] as string;
                return current;
            }
            set
            {
                _localSettings.Values["pollinationsKey"] = value?.Trim();
            }
        }

        private static readonly HttpClient _httpClient = new HttpClient();
        private List<LLMModelInfo> _cachedModels = null;
        private readonly SemaphoreSlim _modelLock = new SemaphoreSlim(1, 1);

        public async Task<IEnumerable<LLMModelInfo>> GetAvailableModelsAsync()
        {
            Debug.WriteLine($"[PollinationsDebug] GetAvailableModelsAsync started.");

            if (_cachedModels != null)
            {
                Debug.WriteLine($"[PollinationsDebug] Returning cached models. Count: {_cachedModels.Count}");
                return _cachedModels;
            }

            await _modelLock.WaitAsync();
            try
            {
                if (_cachedModels != null) return _cachedModels;

                var fetchedModels = new List<LLMModelInfo>();
                string requestUrl = $"{BaseUrl}/models";

                Debug.WriteLine($"[PollinationsDebug] Fetching models from: {requestUrl}");

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", string.IsNullOrWhiteSpace(ApiKey) ? "dummy" : ApiKey);

                using var response = await _httpClient.SendAsync(request);

                Debug.WriteLine($"[PollinationsDebug] Models response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[PollinationsDebug] Failed to fetch models. Status: {response.StatusCode}. Reason: {response.ReasonPhrase}");
                    return new[] { new LLMModelInfo { Name = "error", Description = "🔑 Error fetching models" } };
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[PollinationsDebug] Models raw JSON: {json}");

                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        string modelName = null;
                        string description = null;

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
                else
                {
                    Debug.WriteLine($"[PollinationsDebug] Unexpected JSON root kind: {doc.RootElement.ValueKind}");
                }

                if (fetchedModels.Count == 0)
                {
                    Debug.WriteLine("[PollinationsDebug] Parsed model list is empty. Adding fallback.");
                    fetchedModels.Add(new LLMModelInfo { Name = "openai", Description = "OpenAI" });
                }

                Debug.WriteLine($"[PollinationsDebug] Successfully cached {fetchedModels.Count} models.");
                _cachedModels = fetchedModels;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PollinationsDebug] Model Fetch Exception: {ex}");
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

            Debug.WriteLine($"[PollinationsDebug] StreamChat started. Model: {modelName}");

            var messages = BuildMessages(chatHistory);
            var requestPayload = new
            {
                model = modelName,
                messages = messages,
                stream = true
            };

            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            string url = $"{BaseUrl}/openai/chat/completions";

            Debug.WriteLine($"[PollinationsDebug] POST URL: {url}");
            Debug.WriteLine($"[PollinationsDebug] Payload Preview (Truncated): {jsonPayload.Substring(0, Math.Min(jsonPayload.Length, 200))}...");

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", !string.IsNullOrWhiteSpace(ApiKey) ? ApiKey : "");
            System.Diagnostics.Debug.WriteLine($"[Pollinations] Using Key: '{request.Headers.Authorization.Parameter}'");
            request.Headers.TryAddWithoutValidation("Origin", "FairmarkApp");

            HttpResponseMessage response = null;
            string connectionError = null;

            try
            {
                Debug.WriteLine("[PollinationsDebug] Sending request...");
                response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                Debug.WriteLine($"[PollinationsDebug] Response headers received. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                connectionError = $"Failed to connect: {ex.Message}";
                Debug.WriteLine($"[PollinationsDebug] Connection Exception: {ex}");
            }

            if (connectionError != null)
            {
                yield return new LLMStreamedNote
                {
                    Title = _loader.GetString("Title_ConnectionError"),
                    ContentPart = string.Format(_loader.GetString("Error_ConnectionFailed"), connectionError)
                };
                yield break;
            }

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[PollinationsDebug] HTTP Error Content: {errorContent}");

                yield return new LLMStreamedNote { Title = "API Error", ContentPart = $"API returned {response.StatusCode}: {errorContent}" };
                response.Dispose();
                yield break;
            }

            using (response)
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                Debug.WriteLine("[PollinationsDebug] Starting to read stream...");

                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(line)) continue;


                    if (!line.StartsWith("data: "))
                    {
                        Debug.WriteLine($"[PollinationsDebug] Skipping non-data line: {line}");
                        continue;
                    }

                    var data = line.Substring(6).Trim();
                    if (data == "[DONE]")
                    {
                        Debug.WriteLine("[PollinationsDebug] Received [DONE] signal.");
                        break;
                    }

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
                        else
                        {
                            Debug.WriteLine($"[PollinationsDebug] JSON parsed but no valid content found in choices: {data}");
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        Debug.WriteLine($"[PollinationsDebug] JSON Parse Error for line: '{data}'. Error: {jsonEx.Message}");
                        continue;
                    }

                    if (!string.IsNullOrEmpty(content))
                    {
                        yield return new LLMStreamedNote { Title = "Pollinations AI", ContentPart = content };
                    }
                }
                Debug.WriteLine("[PollinationsDebug] Stream loop finished.");
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
                role = msg.Role.ToString().ToLowerInvariant(),
                content = msg.Content
            }).Cast<object>().ToList();
        }
    }
}
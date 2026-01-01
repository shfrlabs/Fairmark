using Fairmark.Intelligence.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.Intelligence.Providers
{
    public class DebugProvider : ILLMProvider
    {
        public string Name => "Debug provider";

        public string ApiKey { get => ""; set => Debug.WriteLine("Tried to write debug provider api key"); }
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public string LastUsedModel
        {
            get
            {
                string current = _localSettings.Values["debugLastModel"] as string;
                return current;
            }
            set
            {
                _localSettings.Values["debugLastModel"] = value?.Trim();
            }
        }

        public async Task<IEnumerable<LLMModelInfo>> GetAvailableModelsAsync()
        {
            Debug.WriteLine("GetAvailableModelsAsync called");

            await Task.Delay(100);

            return new List<LLMModelInfo>
            {
                new LLMModelInfo { Name = "debug-model", Description = "Debug LLM Model (fake)" }
            };
        }

        public async IAsyncEnumerable<LLMStreamedNote> StreamChat(
            IEnumerable<LLMChatMessage> chatHistory,
            string modelName = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"StreamChat called. modelName: {modelName}");

            foreach (var msg in chatHistory)
            {
                Debug.WriteLine($"ChatMessage: Role={msg.Role}, Content={msg.Content}");
            }

            for (int i = 1; i <= 15; i++)
            {
                if (cancellationToken.IsCancellationRequested) yield break;

                await Task.Delay(100, cancellationToken);

                yield return new LLMStreamedNote
                {
                    Title = "Chat Response Title",
                    ContentPart = $"Chat response part {i}... "
                };
            }
        }

        public IAsyncEnumerable<LLMStreamedNote> StreamCreateNote(
            string promptOrDocument,
            string modelName = null,
            CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"StreamCreateNote called. promptOrDocument: {promptOrDocument}, modelName: {modelName}");

            var history = new List<LLMChatMessage>
            {
                new LLMChatMessage { Role = LLMChatRole.User, Content = promptOrDocument }
            };

            return StreamChat(history, modelName, cancellationToken);
        }

        public IAsyncEnumerable<LLMStreamedNote> StreamSummarizeNote(
            string noteContent,
            string modelName = null,
            CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"StreamSummarizeNote called. noteContent: {noteContent}, modelName: {modelName}");

            var history = new List<LLMChatMessage>
            {
                new LLMChatMessage { Role = LLMChatRole.User, Content = "Summarize: " + noteContent }
            };

            return StreamChat(history, modelName, cancellationToken);
        }
    }
}
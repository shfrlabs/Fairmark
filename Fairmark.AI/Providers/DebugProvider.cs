using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Fairmark.Intelligence.Models;

namespace Fairmark.Intelligence.Providers
{
    public class DebugProvider : Fairmark.Intelligence.ILLMProvider
    {
        public string Name => "Debug provider";

        public IEnumerable<LLMModelInfo> GetAvailableModels()
        {
            Debug.WriteLine("GetAvailableModels called");
            yield return new LLMModelInfo { Name = "debug-model", Description = "Debug LLM Model (fake)" };
        }

        public IEnumerable<LLMStreamedNote> StreamSummarizeNote(string noteContent, string modelName = null, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"StreamSummarizeNote called. noteContent: {noteContent}, modelName: {modelName}");
            for (int i = 1; i <= 3; i++)
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                yield return new LLMStreamedNote
                {
                    Title = "Summary Title",
                    ContentPart = $"Summary part {i} for: {noteContent?.Substring(0, Math.Min(20, noteContent.Length))}..."
                };
            }
        }

        public IEnumerable<LLMStreamedNote> StreamCreateNote(string promptOrDocument, string modelName = null, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"StreamCreateNote called. promptOrDocument: {promptOrDocument}, modelName: {modelName}");
            for (int i = 1; i <= 3; i++)
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                yield return new LLMStreamedNote
                {
                    Title = "Created Note Title",
                    ContentPart = $"Note part {i} for: {promptOrDocument?.Substring(0, Math.Min(20, promptOrDocument.Length))}..."
                };
            }
        }

        public IEnumerable<LLMStreamedNote> StreamChat(IEnumerable<LLMChatMessage> chatHistory, string modelName = null, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"StreamChat called. modelName: {modelName}");
            foreach (var msg in chatHistory)
            {
                Debug.WriteLine($"ChatMessage: Role={msg.Role}, Content={msg.Content}");
            }
            for (int i = 1; i <= 3; i++)
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                yield return new LLMStreamedNote
                {
                    Title = "Chat Response Title",
                    ContentPart = $"Chat response part {i}..."
                };
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Fairmark.Intelligence.Models;

namespace Fairmark.Intelligence
{
    public interface ILLMProvider
    {
        string Name { get; }
        IEnumerable<LLMModelInfo> GetAvailableModels();

        IEnumerable<LLMStreamedNote> StreamSummarizeNote(
            string noteContent,
            string modelName = null,
            CancellationToken cancellationToken = default);

        IEnumerable<LLMStreamedNote> StreamCreateNote(
            string promptOrDocument,
            string modelName = null,
            CancellationToken cancellationToken = default);

        IEnumerable<LLMStreamedNote> StreamChat(
            IEnumerable<LLMChatMessage> chatHistory,
            string modelName = null,
            CancellationToken cancellationToken = default);
    }
}
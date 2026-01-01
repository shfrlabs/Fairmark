using Fairmark.Intelligence.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fairmark.Intelligence
{
    public interface ILLMProvider
    {
        string Name { get; }
        string ApiKey { get; set; }
        string LastUsedModel { get; set; }
        Task<IEnumerable<LLMModelInfo>> GetAvailableModelsAsync();

        IAsyncEnumerable<LLMStreamedNote> StreamSummarizeNote(
            string noteContent,
            string modelName = null,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<LLMStreamedNote> StreamCreateNote(
            string promptOrDocument,
            string modelName = null,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<LLMStreamedNote> StreamChat(
            IEnumerable<LLMChatMessage> chatHistory,
            string modelName = null,
            CancellationToken cancellationToken = default);
    }
}
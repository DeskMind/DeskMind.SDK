using DeskMind.Rag.Abstractions;

using System;

namespace DeskMind.Rag.Retrieval
{
    public sealed class RagRetrieverFactory : IRagRetrieverFactory
    {
        public IRagRetriever Create(
            IVectorMemory memory,
            IEmbeddingGenerator? embeddings = null,
            RagRetrieveOptions? options = null)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));
            return new SimpleRagRetriever(memory, options ?? new RagRetrieveOptions());
        }
    }
}
using DeskMind.Rag.Abstractions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Models
{
    /// <summary>
    /// Simple IRagSource implementation that just wires memory, ingestion and retriever together.
    /// </summary>
    public sealed class RagSource : IRagSource
    {
        public RagSource(
            string name,
            IVectorMemory memory,
            IDocumentIngestionService ingestion,
            IRagRetriever retriever,
            IEmbeddingGenerator embeddings)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Memory = memory ?? throw new ArgumentNullException(nameof(memory));
            Ingestion = ingestion ?? throw new ArgumentNullException(nameof(ingestion));
            Retriever = retriever ?? throw new ArgumentNullException(nameof(retriever));
            Embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
        }

        public string Name { get; }

        public IVectorMemory Memory { get; }

        public IDocumentIngestionService Ingestion { get; }

        public IRagRetriever Retriever { get; }

        public IEmbeddingGenerator Embeddings { get; }

        public static Task InitializeAsync(CancellationToken ct = default)
        {
            // Hook for lazy ingestion or pre-warm if you need it.
            // For now, PythonRunnerKnowledgeFactory does ingestion directly, so nothing to do.
            return Task.CompletedTask;
        }
    }
}
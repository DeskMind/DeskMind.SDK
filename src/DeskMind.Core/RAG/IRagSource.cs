using System;
using System.Collections.Generic;
using System.Text;

namespace DeskMind.Core.RAG
{
    /// <summary>
    /// Aggregates retrieval and ingestion for a single logical RAG source.
    /// </summary>
    public interface IRagSource
    {
        string Name { get; }

        IVectorMemory VectorMemory { get; }

        IDocumentIngestionService Ingestion { get; }

        IRagRetriever Retriever { get; }

        IEmbeddingGenerator Embeddings { get; }
    }
}
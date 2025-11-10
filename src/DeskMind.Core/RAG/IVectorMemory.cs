using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Core.RAG
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Vector Memory (Storage & Similarity)
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Abstraction over a local vector store (SQLite, Postgres/pgvector, Qdrant, Redis, etc.).
    /// Thread-safe implementations are recommended for background ingestion.
    /// </summary>
    public interface IVectorMemory
    {
        /// <summary>Dimensionality expected by this store.</summary>
        int Dimensions { get; }

        /// <summary>Insert or update a single chunk by ID with an already-computed embedding.</summary>
        Task UpsertAsync(
            string chunkId,
            string text,
            Embedding embedding,
            DocumentReference document,
            Metadata? metadata = null,
            CancellationToken ct = default);

        /// <summary>Insert or update multiple chunks in one call (prefer this for performance).</summary>
        Task UpsertBatchAsync(
            IReadOnlyList<(string ChunkId, string Text, Embedding Embedding, DocumentReference Document, Metadata? Metadata)> batch,
            CancellationToken ct = default);

        /// <summary>Delete all chunks belonging to a given document key.</summary>
        Task DeleteByDocumentAsync(string documentKey, CancellationToken ct = default);

        /// <summary>Remove all data from the store.</summary>
        Task PurgeAsync(CancellationToken ct = default);

        /// <summary>Approximate count for diagnostics/UX.</summary>
        Task<long> CountAsync(CancellationToken ct = default);

        /// <summary>
        /// Vector search using a precomputed embedding. Returns highest-similarity chunks.
        /// </summary>
        Task<IReadOnlyList<SearchHit>> SearchByVectorAsync(
            Embedding query,
            int topK = 5,
            IReadOnlyDictionary<string, object?>? filter = null,
            CancellationToken ct = default);

        /// <summary>
        /// Convenience API: embed the <paramref name="query"/> and perform search.
        /// Implementations may use the registered <see cref="IEmbeddingGenerator"/>.
        /// </summary>
        Task<IReadOnlyList<SearchHit>> SearchAsync(
            string query,
            int topK = 5,
            IReadOnlyDictionary<string, object?>? filter = null,
            CancellationToken ct = default);
    }
}
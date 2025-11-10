using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Core.RAG
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Retrieval (Optional Facade)
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Optional high-level retrieval facade that combines vector search with lightweight post-processing
    /// (e.g., deduplication, short rerank, and prompt-ready formatting).
    /// </summary>
    public interface IRagRetriever
    {
        /// <summary>
        /// Retrieve top-K chunks for the given query, ready to be stuffed into a prompt.
        /// </summary>
        Task<IReadOnlyList<SearchHit>> RetrieveAsync(
            string query,
            int topK = 5,
            IReadOnlyDictionary<string, object?>? filter = null,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Represents a hit returned from vector search.
    /// </summary>
    public sealed record SearchHit(
        string ChunkId,
        DocumentReference Document,
        string Text,
        double Score,
        Metadata? Metadata = null);
}
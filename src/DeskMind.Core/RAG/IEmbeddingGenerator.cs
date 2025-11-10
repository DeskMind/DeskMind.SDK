using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Core.RAG
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Embeddings
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Minimal embedding generator contract. Implement with SK, Olive/ONNX, local models, etc.
    /// </summary>
    public interface IEmbeddingGenerator
    {
        /// <summary>Dimensionality of returned vectors.</summary>
        int Dimensions { get; }

        /// <summary>Generate an embedding for a single text.</summary>
        Task<Embedding> GenerateAsync(string text, CancellationToken ct = default);

        /// <summary>Generate embeddings for a batch of texts; preserves order.</summary>
        Task<IReadOnlyList<Embedding>> GenerateBatchAsync(IReadOnlyList<string> texts, CancellationToken ct = default);
    }
}
using System.Collections.Generic;

namespace DeskMind.Core.RAG
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Extraction & Splitting
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Splits a large text into smaller overlapping chunks suitable for embedding.
    /// Keep split decisions deterministic (idempotent) given the same input text.
    /// </summary>
    public interface ITextSplitter
    {
        /// <summary>
        /// Produce chunks with the requested <paramref name="chunkSize"/> and <paramref name="chunkOverlap"/>.
        /// Implementations may use hierarchical rules (paragraphs, sentences) before fallback to character limits.
        /// </summary>
        IEnumerable<SplitChunk> Split(
            ExtractionResult extraction,
            int chunkSize,
            int chunkOverlap);
    }
}
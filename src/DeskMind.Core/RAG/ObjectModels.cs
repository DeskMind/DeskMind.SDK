using System;
using System.Collections.Generic;

namespace DeskMind.Core.RAG
{
    /// <summary>
    /// Ingestion options controlling chunking, deduplication and indexing behavior.
    /// </summary>
    public sealed record IngestOptions(
        int ChunkSize = 1200,
        int ChunkOverlap = 200,
        bool SkipIfUnchanged = true,
        bool NormalizeWhitespace = true,
        IReadOnlyDictionary<string, object?>? DefaultMetadata = null);

    /// <summary>
    /// Describes a single dense embedding vector used for similarity search.
    /// Implementations should respect the expected <see cref="Dimensions"/>.
    /// </summary>
    public readonly record struct Embedding(ReadOnlyMemory<float> Values, int Dimensions);

    /// <summary>
    /// Result of text extraction for a document, including plain text and optional per-page map.
    /// </summary>
    public sealed record ExtractionResult(
        DocumentReference Document,
        string Text,
        IReadOnlyList<string>? Pages = null,
        Metadata? Metadata = null);

    /// <summary>
    /// Represents a single chunk produced by the splitter.
    /// </summary>
    public sealed record SplitChunk(
        DocumentReference Document,
        int Index,
        string Text,
        Metadata? Metadata = null);

    /// <summary>
    /// Simple metadata container associated with a stored chunk or document.
    /// Use for path, page, section, content-type, etc. Values must be JSON-serializable.
    /// </summary>
    public sealed record Metadata(IDictionary<string, object?> Items)
    {
        public static Metadata Empty { get; } = new(new Dictionary<string, object?>());
        public object? this[string key]
        {
            get => Items.TryGetValue(key, out var v) ? v : null;
            init => Items[key] = value;
        }
    }

    /// <summary>
    /// A logical reference to a source document (e.g., a file path or URI).
    /// The <see cref="Key"/> should be stable and unique per source.
    /// </summary>
    public sealed record DocumentReference(string Key, string? DisplayName = null, string? ContentType = null);
}
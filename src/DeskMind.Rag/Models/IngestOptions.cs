using System.Collections.Generic;

namespace DeskMind.Rag.Models
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
}
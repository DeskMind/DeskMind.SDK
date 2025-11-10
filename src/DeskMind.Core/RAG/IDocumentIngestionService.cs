using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Core.RAG
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Ingestion Orchestration
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// High-level ingestion service that extracts, splits, embeds and stores content.
    /// Designed to be used by background watchers and UX flows (drag/drop, “Add folder”, etc.).
    /// </summary>
    public interface IDocumentIngestionService
    {
        /// <summary>
        /// Ingest a single file or URI. Idempotent if chunk IDs include stable content hashes.
        /// </summary>
        Task IngestAsync(string pathOrUri, IngestOptions? options = null, IProgress<string>? progress = null, CancellationToken ct = default);

        /// <summary>
        /// Ingest all matching items in a folder (optionally recursive). Patterns are glob-like (e.g., "*.pdf").
        /// </summary>
        Task IngestFolderAsync(string folderPath, IReadOnlyList<string>? patterns = null, bool recursive = true,
                               IngestOptions? options = null, IProgress<string>? progress = null, CancellationToken ct = default);

        /// <summary>
        /// Rebuild embeddings/records for a document or an entire folder regardless of prior state.
        /// </summary>
        Task ReindexAsync(string pathOrUri, IngestOptions? options = null, IProgress<string>? progress = null, CancellationToken ct = default);

        /// <summary>
        /// Remove all chunks for a given document key (e.g., its absolute path).
        /// </summary>
        Task RemoveAsync(string documentKey, CancellationToken ct = default);
    }
}
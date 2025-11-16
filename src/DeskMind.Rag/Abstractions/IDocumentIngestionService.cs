using DeskMind.Rag.Models;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Abstractions
{
    /// <summary>
    /// High-level ingestion service that extracts, splits, embeds and stores content
    /// into a vector memory. Source can be file paths, URIs or raw text.
    /// </summary>
    public interface IDocumentIngestionService
    {
        /// <summary>
        /// Ingest a single file or URI. Idempotent if chunk IDs include stable content hashes.
        /// </summary>
        Task IngestAsync(
            string pathOrUri,
            IngestOptions? options = null,
            IProgress<string>? progress = null,
            CancellationToken ct = default);

        /// <summary>
        /// Ingest all matching items in a folder.
        /// </summary>
        Task IngestFolderAsync(
            string folderPath,
            IReadOnlyList<string>? patterns = null,
            bool recursive = true,
            IngestOptions? options = null,
            IProgress<string>? progress = null,
            CancellationToken ct = default);

        /// <summary>
        /// Rebuild embeddings/records for a document or a folder regardless of prior state.
        /// </summary>
        Task ReindexAsync(
            string pathOrUri,
            IngestOptions? options = null,
            IProgress<string>? progress = null,
            CancellationToken ct = default);

        /// <summary>
        /// Remove all chunks associated with a logical document key (e.g., its absolute path or URI).
        /// </summary>
        Task RemoveAsync(string documentKey, CancellationToken ct = default);

        /// <summary>
        /// Ingest raw text for a logical document reference (used by embedded knowledge packs).
        /// </summary>
        Task IngestTextAsync(
            DocumentReference document,
            string text,
            IngestOptions? options = null,
            IProgress<string>? progress = null,
            CancellationToken ct = default);
    }
}
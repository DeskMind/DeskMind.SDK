using DeskMind.Rag.Abstractions;
using DeskMind.Rag.Helpers;
using DeskMind.Rag.Models;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Processing.Ingestion
{
    /// <summary>
    /// Default implementation of <see cref="IDocumentIngestionService"/>.
    /// Orchestrates: extract → split → embed → upsert.
    /// </summary>
    public sealed class DocumentIngestionService : IDocumentIngestionService
    {
        private readonly IEnumerable<IContentExtractor> _extractors;
        private readonly ITextSplitter _splitter;
        private readonly IVectorMemory _memory;
        private readonly IEmbeddingGenerator _embeddings;
        private readonly ILogger<DocumentIngestionService> _logger;

        public DocumentIngestionService(
            IEnumerable<IContentExtractor> extractors,
            ITextSplitter splitter,
            IVectorMemory memory,
            IEmbeddingGenerator embeddings,
            ILogger<DocumentIngestionService> logger)
        {
            _extractors = extractors;
            _splitter = splitter;
            _memory = memory;
            _embeddings = embeddings;
            _logger = logger;
        }

        public async Task IngestAsync(
            string pathOrUri,
            IngestOptions? options = null,
            IProgress<string>? progress = null,
            CancellationToken ct = default)
        {
            options ??= new IngestOptions();
            progress?.Report($"Ingesting {pathOrUri} ...");

            var extractor = _extractors.FirstOrDefault(e => e.CanHandle(pathOrUri));
            if (extractor is null)
            {
                _logger.LogWarning("No content extractor can handle '{PathOrUri}'", pathOrUri);
                return;
            }

            var extraction = await extractor.ExtractAsync(pathOrUri, ct).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(extraction.Text))
            {
                _logger.LogInformation("Skipping empty document {Key}", extraction.Document.Key);
                return;
            }

            await IngestCoreAsync(extraction, options, progress, ct).ConfigureAwait(false);
        }

        public async Task IngestFolderAsync(
            string folderPath,
            IReadOnlyList<string>? patterns = null,
            bool recursive = true,
            IngestOptions? options = null,
            IProgress<string>? progress = null,
            CancellationToken ct = default)
        {
            options ??= new IngestOptions();
            patterns ??= new[] { "*.txt", "*.md", "*.pdf" };

            if (!Directory.Exists(folderPath))
            {
                _logger.LogWarning("Folder {Folder} does not exist", folderPath);
                return;
            }

            foreach (var pattern in patterns)
            {
                var files = Directory.EnumerateFiles(
                    folderPath,
                    pattern,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    ct.ThrowIfCancellationRequested();
                    await IngestAsync(file, options, progress, ct).ConfigureAwait(false);
                }
            }
        }

        public Task ReindexAsync(
            string pathOrUri,
            IngestOptions? options = null,
            IProgress<string>? progress = null,
            CancellationToken ct = default)
        {
            options ??= new IngestOptions();
            // Just re-run ingestion, but caller can pass options with SkipIfUnchanged = false
            return IngestAsync(pathOrUri, options with { SkipIfUnchanged = false }, progress, ct);
        }

        public Task RemoveAsync(string documentKey, CancellationToken ct = default)
        {
            _logger.LogInformation("Removing all chunks for document {Key}", documentKey);
            return _memory.DeleteByDocumentAsync(documentKey, ct);
        }

        public async Task IngestTextAsync(
            DocumentReference document,
            string text,
            IngestOptions? options = null,
            IProgress<string>? progress = null,
            CancellationToken ct = default)
        {
            options ??= new IngestOptions();
            progress?.Report($"Ingesting text for {document.Key} ...");

            if (options.NormalizeWhitespace)
            {
                text = NormalizeWhitespace(text);
            }

            var extraction = new ExtractionResult(document, text);
            await IngestCoreAsync(extraction, options, progress, ct).ConfigureAwait(false);
        }

        private async Task IngestCoreAsync(
            ExtractionResult extraction,
            IngestOptions options,
            IProgress<string>? progress,
            CancellationToken ct)
        {
            var doc = extraction.Document;
            var chunks = _splitter.Split(extraction, options.ChunkSize, options.ChunkOverlap).ToList();
            if (chunks.Count == 0)
            {
                _logger.LogInformation("No chunks produced for document {Key}", doc.Key);
                return;
            }

            var texts = chunks.Select(c => c.Text).ToArray();
            var embeddings = await _embeddings.GenerateBatchAsync(texts, ct).ConfigureAwait(false);

            // Ids: stable based on document key + chunk index + chunk hash
            var batch = new List<(string ChunkId, string Text, Embedding Embedding, DocumentReference Document, Metadata? Metadata)>(chunks.Count);

            for (int i = 0; i < chunks.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var c = chunks[i];
                var emb = embeddings[i];

                var contentHash = HashUtil.Sha256(c.Text);
                var id = $"{doc.Key}::{c.Index}::{contentHash}";

                var md = c.Metadata ?? Metadata.Empty;
                batch.Add((id, c.Text, emb, c.Document, md));
            }

            await _memory.UpsertBatchAsync(batch, ct).ConfigureAwait(false);

            _logger.LogInformation("Ingested {Count} chunks for document {Key}", batch.Count, doc.Key);
            progress?.Report($"Ingested {batch.Count} chunks for {doc.Key}");
        }

        private static string NormalizeWhitespace(string text)
        {
            // Normalize line endings and collapse excessive blank lines
            text = text.Replace("\r\n", "\n");
            text = Regex.Replace(text, @"[ \t]+\n", "\n");
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            return text.Trim();
        }
    }

    internal static class HashUtil
    {
        public static string Sha256(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return CompatibilityExtensions.ToHexString(hash);
        }
    }
}
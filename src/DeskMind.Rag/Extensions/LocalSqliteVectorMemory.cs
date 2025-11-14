using DeskMind.Rag.Abstractions;
using DeskMind.Rag.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Extensions
{
    /// <summary>
    /// Default local vector memory implementation backed by a SQLite vector store,
    /// using VectorStoreCollection&lt;string, VectorDocument&gt; and your IEmbeddingGenerator.
    /// </summary>
    public sealed class LocalSqliteVectorMemory : IVectorMemory, IAsyncDisposable
    {
        private readonly VectorStoreCollection<string, VectorDocument> _collection;
        private readonly IEmbeddingGenerator _embeddings;
        private readonly ILogger<LocalSqliteVectorMemory> _logger;
        private bool _collectionEnsured;

        public LocalSqliteVectorMemory(
            VectorStoreCollection<string, VectorDocument> collection,
            IEmbeddingGenerator embeddings,
            ILogger<LocalSqliteVectorMemory> logger)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Name = "local";
        }

        /// <summary>Logical name used by IVectorMemoryResolver.</summary>
        public string Name { get; }

        /// <summary>Dimensionality = whatever your embedding generator uses.</summary>
        public int Dimensions => _embeddings.Dimensions;

        // ─────────────────────────────────────────────────────────────
        // Upsert
        // ─────────────────────────────────────────────────────────────

        public async Task UpsertAsync(
            string chunkId,
            string text,
            Embedding embedding,
            DocumentReference document,
            Metadata? metadata = null,
            CancellationToken ct = default)
        {
            if (chunkId is null) throw new ArgumentNullException(nameof(chunkId));
            if (text is null) throw new ArgumentNullException(nameof(text));
            if (embedding == null) throw new ArgumentNullException(nameof(embedding));
            if (document is null) throw new ArgumentNullException(nameof(document));

            await EnsureCollectionExistsAsync(ct).ConfigureAwait(false);

            var record = CreateRecord(chunkId, text, embedding, document, metadata);
            await _collection.UpsertAsync(record, ct).ConfigureAwait(false);
        }

        public async Task UpsertBatchAsync(
            IReadOnlyList<(string ChunkId, string Text, Embedding Embedding, DocumentReference Document, Metadata? Metadata)> batch,
            CancellationToken ct = default)
        {
            if (batch == null) throw new ArgumentNullException(nameof(batch));
            if (batch.Count == 0) return;

            await EnsureCollectionExistsAsync(ct).ConfigureAwait(false);

            var records = new List<VectorDocument>(batch.Count);
            for (int i = 0; i < batch.Count; i++)
            {
                var item = batch[i];
                records.Add(CreateRecord(
                    item.ChunkId,
                    item.Text,
                    item.Embedding,
                    item.Document,
                    item.Metadata));
            }

            await _collection.UpsertAsync(records, ct).ConfigureAwait(false);
        }

        // ─────────────────────────────────────────────────────────────
        // Delete / Purge / Count
        // ─────────────────────────────────────────────────────────────

        public async Task DeleteByDocumentAsync(string documentKey, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(documentKey))
                throw new ArgumentException("Document key must not be null or empty.", nameof(documentKey));

            await EnsureCollectionExistsAsync(ct).ConfigureAwait(false);

            // Use the filter-based GetAsync:
            // GetAsync(Expression<Func<TRecord,bool>>, int top, FilteredRecordRetrievalOptions<TRecord>?, CancellationToken) :contentReference[oaicite:2]{index=2}
            Expression<Func<VectorDocument, bool>> filter = d => d.DocumentKey == documentKey;
            var options = new FilteredRecordRetrievalOptions<VectorDocument>();

            var ids = new List<string>();

            await foreach (var record in _collection
                .GetAsync(filter, int.MaxValue, options, ct)
                .ConfigureAwait(false))
            {
                if (!string.IsNullOrEmpty(record.Id))
                {
                    ids.Add(record.Id);
                }
            }

            if (ids.Count == 0)
            {
                _logger.LogDebug(
                    "No chunks found for document key '{DocumentKey}' in collection {CollectionName}.",
                    documentKey, _collection.Name);
                return;
            }

            _logger.LogInformation(
                "Deleting {Count} chunks for document key '{DocumentKey}' from collection {CollectionName}.",
                ids.Count, documentKey, _collection.Name);

            await _collection.DeleteAsync(ids, ct).ConfigureAwait(false);
        }

        public async Task PurgeAsync(CancellationToken ct = default)
        {
            // VectorStoreCollection has EnsureCollectionDeletedAsync(CancellationToken) :contentReference[oaicite:3]{index=3}
            _logger.LogWarning(
                "Purging all records from local SQLite vector collection '{CollectionName}'.",
                _collection.Name);

            await _collection.EnsureCollectionDeletedAsync(ct).ConfigureAwait(false);

            _collectionEnsured = false;
        }

        public async Task<long> CountAsync(CancellationToken ct = default)
        {
            await EnsureCollectionExistsAsync(ct).ConfigureAwait(false);

            long count = 0;

            // Full scan: GetAsync(filter:true) with a high 'top' and count the records.
            Expression<Func<VectorDocument, bool>> filter = d => true;
            var options = new FilteredRecordRetrievalOptions<VectorDocument>();

            await foreach (var _ in _collection
                .GetAsync(filter, int.MaxValue, options, ct)
                .ConfigureAwait(false))
            {
                count++;
            }

            return count;
        }

        // ─────────────────────────────────────────────────────────────
        // Search
        // ─────────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<SearchHit>> SearchByVectorAsync(
            Embedding query,
            int topK = 5,
            IReadOnlyDictionary<string, object?>? filter = null,
            CancellationToken ct = default)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (topK <= 0) throw new ArgumentOutOfRangeException(nameof(topK));

            await EnsureCollectionExistsAsync(ct).ConfigureAwait(false);

            var options = new VectorSearchOptions<VectorDocument>();
            // If you want to add filters later, you can map 'filter' into VectorSearchFilter here. :contentReference[oaicite:4]{index=4}

            var results = new List<SearchHit>();

            // VectorStoreCollection.SearchAsync<TInput>(TInput, int top, VectorSearchOptions<TRecord>?, CancellationToken) :contentReference[oaicite:5]{index=5}
            await foreach (var result in _collection
                .SearchAsync<ReadOnlyMemory<float>>(query.Values, topK, options, ct)
                .ConfigureAwait(false))
            {
                var record = result.Record;

                var docRef = new DocumentReference(
                    Key: record.DocumentKey,
                    DisplayName: record.DisplayName,
                    ContentType: record.ContentType);

                Metadata meta = Metadata.Empty;
                if (!string.IsNullOrEmpty(record.MetadataJson))
                {
                    try
                    {
                        // Use nullable object values to match Metadata constructor
                        var dict = JsonSerializer
                            .Deserialize<Dictionary<string, object?>>(record.MetadataJson!);

                        if (dict != null && dict.Count > 0)
                        {
                            meta = new Metadata(dict);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to deserialize metadata JSON for record {Id}.",
                            record.Id);
                    }
                }

                results.Add(new SearchHit(
                    ChunkId: record.Id,
                    Document: docRef,
                    Text: record.Text,
                    Score: result.Score ?? 0.0,
                    Metadata: meta));
            }

            return results;
        }

        public async Task<IReadOnlyList<SearchHit>> SearchAsync(
            string query,
            int topK = 5,
            IReadOnlyDictionary<string, object?>? filter = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query must not be null or empty.", nameof(query));

            var emb = await _embeddings.GenerateAsync(query, ct).ConfigureAwait(false);
            return await SearchByVectorAsync(emb, topK, filter, ct).ConfigureAwait(false);
        }

        // ─────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────

        private VectorDocument CreateRecord(
            string chunkId,
            string text,
            Embedding embedding,
            DocumentReference document,
            Metadata? metadata)
        {
            var record = new VectorDocument
            {
                Id = chunkId,
                Text = text,
                DocumentKey = document.Key ?? string.Empty,
                DisplayName = document.DisplayName,
                ContentType = document.ContentType,
                Embedding = embedding.Values
            };

            if (metadata != null && metadata.Items.Count > 0)
            {
                record.MetadataJson = JsonSerializer.Serialize(metadata.Items);
            }

            return record;
        }

        private async Task EnsureCollectionExistsAsync(CancellationToken ct)
        {
            if (_collectionEnsured)
                return;

            // VectorStoreCollection.EnsureCollectionExistsAsync(CancellationToken) :contentReference[oaicite:6]{index=6}
            await _collection.EnsureCollectionExistsAsync(ct).ConfigureAwait(false);
            _collectionEnsured = true;

            _logger.LogInformation(
                "Ensured local SQLite vector collection '{CollectionName}' exists.",
                _collection.Name);
        }

        public ValueTask DisposeAsync()
        {
            _collection.Dispose();
            return default;
        }
    }
}
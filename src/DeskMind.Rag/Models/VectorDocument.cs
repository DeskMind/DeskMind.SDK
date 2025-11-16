using Microsoft.Extensions.VectorData;

using System;

namespace DeskMind.Rag.Models
{
    /// <summary>
    /// Default record shape stored in a vector collection.
    /// One instance = one chunk.
    /// </summary>
    public class VectorDocument
    {
        /// <summary>
        /// Unique chunk id (documentKey::chunkIndex::hash).
        /// </summary>
        [VectorStoreKey]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Chunk text content (used for retrieval & display).
        /// Full-text indexed so we can optionally combine vector + keyword search.
        /// </summary>
        [VectorStoreData(IsFullTextIndexed = true)]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Logical document identifier (maps back to <see cref="DocumentReference.Key"/>).
        /// Indexed so we can filter / delete by document.
        /// </summary>
        [VectorStoreData(IsIndexed = true)]
        public string DocumentKey { get; set; } = string.Empty;

        /// <summary>
        /// Optional human friendly name of the source file / resource.
        /// </summary>
        [VectorStoreData]
        public string? DisplayName { get; set; }

        /// <summary>
        /// MIME type of the original content (text/markdown, application/pdf, ...).
        /// </summary>
        [VectorStoreData]
        public string? ContentType { get; set; }

        /// <summary>
        /// Arbitrary metadata as JSON (original path, tags, plugin name, etc.).
        /// </summary>
        [VectorStoreData]
        public string? MetadataJson { get; set; }

        /// <summary>
        /// Embedding vector for this chunk.
        /// Dimensions must match the embedding model you use.
        /// </summary>
        [VectorStoreVector(
            Dimensions: RagEmbeddingConfig.Dimensions,
            DistanceFunction = DistanceFunction.CosineDistance,
            IndexKind = IndexKind.Hnsw)]
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace DeskMind.Rag.Models
{
    /// <summary>
    /// Lightweight result type for RAG retrieval.
    /// </summary>
    public sealed class RagSearchResult
    {
        public RagSearchResult(
            VectorDocument document,
            double? score)
        {
            Document = document;
            Score = score;
        }

        /// <summary>
        /// The vector document that matched the query.
        /// </summary>
        public VectorDocument Document { get; }

        /// <summary>
        /// Similarity score from the underlying vector store (if provided).
        /// </summary>
        public double? Score { get; }
    }
}
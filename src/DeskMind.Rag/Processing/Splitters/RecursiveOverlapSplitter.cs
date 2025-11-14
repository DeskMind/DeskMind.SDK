using DeskMind.Rag.Abstractions;
using DeskMind.Rag.Helpers;
using DeskMind.Rag.Models;

using System;
using System.Collections.Generic;

namespace DeskMind.Rag.Processing.Splitters
{
    /// <summary>
    /// Iterative splitter:
    /// - First splits text into paragraph-like segments (by "\n\n").
    /// - If a segment is still too large, it falls back to fixed-size chunks with overlap.
    /// - No recursion, no risk of StackOverflow.
    /// </summary>
    public sealed class RecursiveOverlapSplitter : ITextSplitter
    {
        private readonly int _defaultMaxChars;
        private readonly int _defaultOverlapChars;
        private readonly string _paragraphSeparator;

        public RecursiveOverlapSplitter(
            int defaultMaxChars = 1000,
            int defaultOverlapChars = 200,
            string paragraphSeparator = "\n\n")
        {
            if (defaultMaxChars <= 0) throw new ArgumentOutOfRangeException(nameof(defaultMaxChars));
            if (defaultOverlapChars < 0) throw new ArgumentOutOfRangeException(nameof(defaultOverlapChars));
            if (defaultOverlapChars >= defaultMaxChars)
                throw new ArgumentException("defaultOverlapChars must be < defaultMaxChars.");

            _defaultMaxChars = defaultMaxChars;
            _defaultOverlapChars = defaultOverlapChars;
            _paragraphSeparator = paragraphSeparator;
        }

        /// <summary>
        /// Split the extracted text into ordered chunks.
        /// </summary>
        /// <param name="extraction">Extraction result containing the full text and its document reference.</param>
        /// <param name="maxChunkChars">
        /// Maximum characters per chunk. If &lt;= 0, the splitter falls back to its default.
        /// </param>
        /// <param name="overlapChars">
        /// Overlap in characters between consecutive chunks. If &lt; 0, default is used.
        /// </param>
        public IEnumerable<SplitChunk> Split(
            ExtractionResult extraction,
            int maxChunkChars,
            int overlapChars)
        {
            if (extraction == null) throw new ArgumentNullException(nameof(extraction));

            // Use your actual text property here. If it's different, change this line:
            var text = extraction.Text ?? string.Empty;

            // Adjust from parameters or defaults
            int maxChars = maxChunkChars > 0 ? maxChunkChars : _defaultMaxChars;
            int overlap = overlapChars >= 0 ? overlapChars : _defaultOverlapChars;

            if (overlap >= maxChars)
                overlap = Math.Max(0, maxChars - 1);

            var chunks = new List<SplitChunk>();
            int chunkIndex = 0;

            if (string.IsNullOrWhiteSpace(text))
                return chunks;

            // 1) First break into paragraph-like segments
            var paragraphs = text.SplitIntoSegments(_paragraphSeparator, maxChars * 4);

            foreach (var para in paragraphs)
            {
                if (string.IsNullOrWhiteSpace(para))
                    continue;

                if (para.Length <= maxChars)
                {
                    AddChunk(extraction, para, ref chunkIndex, chunks);
                    continue;
                }

                // 2) Paragraph too big → fallback to fixed-size chunks with overlap
                foreach (var chunkText in FixedSizeWithOverlap(para, maxChars, overlap))
                {
                    AddChunk(extraction, chunkText, ref chunkIndex, chunks);
                }
            }

            return chunks;
        }

        private static void AddChunk(
            ExtractionResult extraction,
            string text,
            ref int index,
            List<SplitChunk> list)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Adjust 'extraction.Document' if your property name is different.
            var docRef = extraction.Document;

            var chunk = new SplitChunk(
                index,              // first int (e.g. ChunkIndex)
                docRef,             // DocumentReference
                index,              // second int (e.g. Order or Offset)
                text.Trim(),        // Text
                null);              // Metadata? (none for now)

            list.Add(chunk);
            index++;
        }

        /// <summary>
        /// Splits a long string into fixed-size chunks with character overlap.
        /// Non-recursive, guaranteed to terminate.
        /// </summary>
        private static IEnumerable<string> FixedSizeWithOverlap(string text, int size, int overlap)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (overlap < 0) throw new ArgumentOutOfRangeException(nameof(overlap));
            if (overlap >= size) throw new ArgumentException("overlap must be < size.");

            int total = text.Length;
            int step = size - overlap;

            for (int i = 0; i < total; i += step)
            {
                int len = Math.Min(size, total - i);
                var part = text.Substring(i, len).Trim();
                if (part.Length > 0)
                    yield return part;
            }
        }
    }
}
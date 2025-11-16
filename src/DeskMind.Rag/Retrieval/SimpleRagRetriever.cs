using DeskMind.Rag.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Retrieval
{
    /// <summary>
    /// Minimal retriever that delegates to IVectorMemory and performs
    /// basic deduplication and whitespace trimming.
    /// </summary>
    public sealed class SimpleRagRetriever : IRagRetriever
    {
        private readonly IVectorMemory _memory;
        private readonly RagRetrieveOptions _options;

        public SimpleRagRetriever(IVectorMemory memory, RagRetrieveOptions options)
        {
            _memory = memory ?? throw new ArgumentNullException(nameof(memory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<IReadOnlyList<SearchHit>> RetrieveAsync(
            string query,
            int topK = 5,
            IReadOnlyDictionary<string, object?>? filter = null,
            CancellationToken ct = default)
        {
            var hits = await _memory.SearchAsync(query, topK, filter, ct).ConfigureAwait(false);

            var list = hits.ToList();

            if (_options.EnableDedup)
            {
                list = list
                    .GroupBy(h => (h.Document.Key ?? string.Empty, Normalize(h.Text)))
                    .Select(g => g.OrderByDescending(x => x.Score).First())
                    .ToList();
            }

            if (_options.EnableTextTrim)
            {
                list = list.Select(h => h with { Text = h.Text.Trim() }).ToList();
            }

            return list;
        }

        private static string Normalize(string text) =>
            (text ?? string.Empty).Trim().Replace("\r\n", "\n");
    }
}
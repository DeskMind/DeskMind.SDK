using DeskMind.Rag.Abstractions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Hosting
{
    public sealed class RagHub : IRagHub
    {
        private readonly IVectorMemoryResolver _memoryResolver;

        private readonly ConcurrentDictionary<string, IRagSource> _sources =
            new(StringComparer.OrdinalIgnoreCase);

        public RagHub(IVectorMemoryResolver memoryResolver /*, other services */)
        {
            _memoryResolver = memoryResolver;
        }

        public IReadOnlyCollection<string> Sources => [.. _sources.Keys];

        public IRagSource Get(string name) =>
            _sources.TryGetValue(name, out var src) ? src :
            throw new KeyNotFoundException($"RAG source '{name}' not found.");

        public bool TryGet(string name, out IRagSource? source) =>
            _sources.TryGetValue(name, out source);

        public void Register(string name, IRagSource source) =>
            _sources[name] = source;

        public bool Remove(string name) =>
            _sources.TryRemove(name, out _);

        public Task<IReadOnlyList<SearchHit>> SearchAsync(
            string source, string query, int top = 5,
            IReadOnlyDictionary<string, object?>? filter = null,
            CancellationToken ct = default)
        {
            var s = Get(source);
            return s.Retriever.RetrieveAsync(query, top, filter, ct);
        }
    }
}
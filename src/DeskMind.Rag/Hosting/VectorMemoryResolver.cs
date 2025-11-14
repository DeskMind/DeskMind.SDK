using DeskMind.Rag.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

namespace DeskMind.Rag.Hosting
{
    /// <summary>
    /// Default implementation of <see cref="IVectorMemoryResolver"/>.
    ///
    /// It is intentionally simple:
    /// - Collects all <see cref="IVectorMemory"/> from DI.
    /// - Picks a default memory (prefers "local" or "default").
    /// - Allows resolving by logical name.
    /// </summary>
    public sealed class VectorMemoryResolver : IVectorMemoryResolver
    {
        private readonly IReadOnlyDictionary<string, IVectorMemory> _memories;
        private readonly ILogger<VectorMemoryResolver> _logger;

        public VectorMemoryResolver(
            IEnumerable<IVectorMemory> memories,
            ILogger<VectorMemoryResolver> logger)
        {
            if (memories == null) throw new ArgumentNullException(nameof(memories));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var dict = new Dictionary<string, IVectorMemory>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var memory in memories)
            {
                if (memory == null) continue;

                if (string.IsNullOrWhiteSpace(memory.Name))
                {
                    _logger.LogWarning(
                        "Vector memory instance {Type} has no Name. " +
                        "It will only be used as a fallback, not resolvable by name.",
                        memory.GetType().FullName);
                    continue;
                }

                if (dict.ContainsKey(memory.Name))
                {
                    _logger.LogWarning(
                        "Duplicate vector memory name '{Name}' detected. " +
                        "Keeping the first instance ({FirstType}), ignoring ({SecondType}).",
                        memory.Name,
                        dict[memory.Name].GetType().FullName,
                        memory.GetType().FullName);
                    continue;
                }

                dict[memory.Name] = memory;
            }

            _memories = dict;

            Default = SelectDefaultMemory(_memories.Values);
            if (Default == null)
            {
                throw new InvalidOperationException(
                    "No IVectorMemory instances registered. " +
                    "Register at least one (e.g. LocalSqliteVectorMemory).");
            }

            _logger.LogInformation(
                "VectorMemoryResolver initialized with {Count} memories. Default = {DefaultName} ({DefaultType}).",
                _memories.Count,
                Default.Name,
                Default.GetType().FullName);
        }

        public IVectorMemory Default { get; }

        public IVectorMemory Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Default;

            if (_memories.TryGetValue(name, out var memory))
                return memory;

            _logger.LogWarning(
                "Requested vector memory '{Name}' not found. Falling back to default '{DefaultName}'.",
                name,
                Default.Name);

            return Default;
        }

        public bool TryGet(string name, out IVectorMemory? memory)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                memory = Default;
                return true;
            }

            if (_memories.TryGetValue(name, out var found))
            {
                memory = found;
                return true;
            }

            memory = null;
            return false;
        }

        private static IVectorMemory SelectDefaultMemory(IEnumerable<IVectorMemory> memories)
        {
            // Prefer logical names "local" or "default"
            var list = memories.ToList();

            var local = list.FirstOrDefault(m =>
                string.Equals(m.Name, "local", StringComparison.OrdinalIgnoreCase));
            if (local != null) return local;

            var def = list.FirstOrDefault(m =>
                string.Equals(m.Name, "default", StringComparison.OrdinalIgnoreCase));
            if (def != null) return def;

            // Fallback: first registered
            return list.FirstOrDefault();
        }
    }
}
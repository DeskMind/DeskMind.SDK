using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Abstractions
{
    public interface IRagHub
    {
        /// <summary>Active source identifiers (e.g., "local", "project").</summary>
        IReadOnlyCollection<string> Sources { get; }

        /// <summary>Get a source or throw if it doesn't exist.</summary>
        IRagSource Get(string name);

        /// <summary>Try to get a source without throwing.</summary>
        bool TryGet(string name, out IRagSource? source);

        /// <summary>Register or replace a source at runtime.</summary>
        void Register(string name, IRagSource source);

        /// <summary>Remove a source if present.</summary>
        bool Remove(string name);

        // Convenience sugar
        Task<IReadOnlyList<SearchHit>> SearchAsync(
            string source, string query, int top = 5,
            IReadOnlyDictionary<string, object?>? filter = null,
            CancellationToken ct = default);
    }
}
using System.Collections.Generic;

namespace DeskMind.Rag.Models
{
    /// <summary>
    /// Simple metadata container associated with a stored chunk or document.
    /// Use for path, page, section, content-type, etc. Values must be JSON-serializable.
    /// </summary>
    public sealed record Metadata(IDictionary<string, object?> Items)
    {
        public static Metadata Empty { get; } = new(new Dictionary<string, object?>());
        public object? this[string key]
        {
            get => Items.TryGetValue(key, out var v) ? v : null;
            init => Items[key] = value;
        }
    }
}
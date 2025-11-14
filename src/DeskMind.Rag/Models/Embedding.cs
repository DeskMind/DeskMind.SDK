using System;

namespace DeskMind.Rag.Models
{
    /// <summary>
    /// Describes a single dense embedding vector used for similarity search.
    /// Implementations should respect the expected <see cref="Dimensions"/>.
    /// </summary>
    public sealed class Embedding
    {
        public Embedding(float[] values)
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public float[] Values { get; }
    }
}
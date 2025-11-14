using System;

namespace DeskMind.Rag.Models
{
    /// <summary>
    /// Describes a single dense embedding vector used for similarity search.
    /// Implementations should respect the expected <see cref="Dimensions"/>.
    /// </summary>
    public readonly record struct Embedding(ReadOnlyMemory<float> Values, int Dimensions);
}
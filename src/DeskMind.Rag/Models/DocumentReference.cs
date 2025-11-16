namespace DeskMind.Rag.Models
{
    /// <summary>
    /// A logical reference to a source document (e.g., a file path or URI).
    /// The <see cref="Key"/> should be stable and unique per source.
    /// </summary>
    public sealed record DocumentReference(string Key, string? DisplayName = null, string? ContentType = null);
}
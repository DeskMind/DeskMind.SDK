namespace DeskMind.Rag.Models
{
    /// <summary>
    /// Represents a single chunk produced by the splitter.
    /// </summary>
    public sealed record SplitChunk(
        int Id,
        DocumentReference Document,
        int Index,
        string Text,
        Metadata? Metadata = null);
}
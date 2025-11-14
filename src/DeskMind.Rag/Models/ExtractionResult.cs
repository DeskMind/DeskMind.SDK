using System.Collections.Generic;

namespace DeskMind.Rag.Models
{
    /// <summary>
    /// Result of text extraction for a document, including plain text and optional per-page map.
    /// </summary>
    public sealed record ExtractionResult(
        DocumentReference Document,
        string Text,
        IReadOnlyList<string>? Pages = null,
        Metadata? Metadata = null);
}
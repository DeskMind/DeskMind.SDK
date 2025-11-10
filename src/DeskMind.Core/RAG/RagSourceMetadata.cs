namespace DeskMind.Core.RAG
{
    public sealed record RagSourceMetadata(
           string Name,
           string DisplayName,
           string? Description = null,
           string? IconGlyph = null); // keep UI hints symmetrical with tool metadata
}
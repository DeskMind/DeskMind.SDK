namespace DeskMind.Rag.Models
{
    public sealed record RagSourceMetadata(
         string Name,
         string DisplayName,
         string? Description = null,
         string? IconGlyph = null);
}
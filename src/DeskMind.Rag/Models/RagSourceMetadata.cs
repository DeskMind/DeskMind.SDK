namespace DeskMind.Rag.Models
{
    public sealed record RagSourceMetadata(
           string Name,
           string DisplayName,
           string? Description = null,
           string? IconGlyph = null,

           // NEW: name of the vector memory to bind to ("local", "project", etc.)
           string? VectorStoreName = null,

           // NEW: a knowledge pack normally means "static embedded docs"
           bool IsKnowledgePack = false,

           // NEW: optional versioning for updates
           string? Version = null,

           // NEW: optional category or tags
           string[]? Tags = null
       );
}
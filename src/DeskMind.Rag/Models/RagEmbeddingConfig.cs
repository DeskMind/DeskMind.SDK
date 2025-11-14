namespace DeskMind.Rag.Models
{
    /// <summary>
    /// Global configuration for RAG embeddings.
    /// Adjust Dimensions to match your embedding model (e.g. 384, 768, 1536).
    /// </summary>
    public static class RagEmbeddingConfig
    {
        public const int Dimensions = 1536; // e.g. OpenAI text-embedding-3-large
    }
}
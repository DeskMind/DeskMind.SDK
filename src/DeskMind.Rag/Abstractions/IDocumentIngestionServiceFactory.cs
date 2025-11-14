namespace DeskMind.Rag.Abstractions
{
    /// <summary>
    /// Creates ingestion services scoped to a particular vector memory / embedding configuration.
    /// </summary>
    public interface IDocumentIngestionServiceFactory
    {
        IDocumentIngestionService Create(
            IVectorMemory memory,
            IEmbeddingGenerator embeddings);
    }
}
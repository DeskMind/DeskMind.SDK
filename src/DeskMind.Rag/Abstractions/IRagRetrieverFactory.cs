namespace DeskMind.Rag.Abstractions
{
    public interface IRagRetrieverFactory
    {
        IRagRetriever Create(
            IVectorMemory memory,
            IEmbeddingGenerator? embeddings = null,
            RagRetrieveOptions? options = null);
    }

    public sealed record RagRetrieveOptions(
        bool EnableDedup = true,
        bool EnableTextTrim = true);
}
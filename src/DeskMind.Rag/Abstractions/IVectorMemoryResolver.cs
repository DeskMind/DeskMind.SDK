namespace DeskMind.Rag.Abstractions
{
    public interface IVectorMemoryResolver
    {
        IVectorMemory Get(string name);
    }
}
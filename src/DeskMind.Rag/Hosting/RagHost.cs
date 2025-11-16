using DeskMind.Rag.Abstractions;

namespace DeskMind.Rag.Hosting
{
    /// <summary>
    /// Simple host-side bridge to expose the IRagHub to plugins
    /// that are instantiated without DI.
    /// Set this once at startup from your host.
    /// </summary>
    public static class RagHost
    {
        public static IRagHub? Hub { get; set; }
    }
}
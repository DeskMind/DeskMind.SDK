using DeskMind.Rag.Abstractions;
using DeskMind.Rag.Hosting;
using DeskMind.Rag.Processing.ContentExtractors;
using DeskMind.Rag.Processing.Ingestion;
using DeskMind.Rag.Processing.Splitters;
using DeskMind.Rag.Retrieval;

using Microsoft.Extensions.DependencyInjection;

namespace DeskMind.Rag.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the core DeskMind.Rag services (hub, registry, extractors, splitter, ingestion, retrieval).
        /// Does NOT register any concrete vector store or embedding implementation.
        /// </summary>
        public static IServiceCollection AddDeskMindRag(this IServiceCollection services)
        {
            // Hosting
            services.AddSingleton<IRagHub, RagHub>();
            services.AddSingleton<RagSourceRegistry>();
            services.AddSingleton<IVectorMemoryResolver, VectorMemoryResolver>();

            // Extractors
            services.AddSingleton<IContentExtractor, TextExtractor>();
            services.AddSingleton<IContentExtractor, MarkdownExtractor>();
            // PdfExtractor is optional, depends on Pdf library:
            services.AddSingleton<IContentExtractor, PdfExtractor>();

            // Splitter
            services.AddSingleton<ITextSplitter, RecursiveOverlapSplitter>();

            // Ingestion
            services.AddSingleton<IDocumentIngestionServiceFactory, DocumentIngestionServiceFactory>();

            // Retrieval
            services.AddSingleton<IRagRetrieverFactory, RagRetrieverFactory>();

            return services;
        }
    }
}
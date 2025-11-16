using DeskMind.Rag.Abstractions;
using DeskMind.Rag.Models;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Processing.Ingestion
{
    public sealed class DocumentIngestionServiceFactory : IDocumentIngestionServiceFactory
    {
        private readonly IEnumerable<IContentExtractor> _extractors;
        private readonly ITextSplitter _splitter;
        private readonly ILoggerFactory _loggerFactory;

        public DocumentIngestionServiceFactory(
            IEnumerable<IContentExtractor> extractors,
            ITextSplitter splitter,
            ILoggerFactory loggerFactory)
        {
            _extractors = extractors;
            _splitter = splitter;
            _loggerFactory = loggerFactory;
        }

        public IDocumentIngestionService Create(
            IVectorMemory memory,
            IEmbeddingGenerator embeddings)
        {
            var logger = _loggerFactory.CreateLogger<DocumentIngestionService>();
            return new DocumentIngestionService(_extractors, _splitter, memory, embeddings, logger);
        }
    }
}
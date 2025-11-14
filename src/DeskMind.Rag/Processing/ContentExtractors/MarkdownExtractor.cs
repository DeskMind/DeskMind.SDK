using DeskMind.Rag.Abstractions;
using DeskMind.Rag.Models;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Processing.ContentExtractors
{
    public sealed class MarkdownExtractor : IContentExtractor
    {
        public bool CanHandle(string pathOrUri)
        {
            if (Uri.TryCreate(pathOrUri, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                return Path.GetExtension(uri.LocalPath).Equals(".md", StringComparison.OrdinalIgnoreCase);
            }

            if (File.Exists(pathOrUri))
            {
                return Path.GetExtension(pathOrUri).Equals(".md", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public Task<ExtractionResult> ExtractAsync(string pathOrUri, CancellationToken ct = default)
        {
            var path = pathOrUri;
            if (Uri.TryCreate(pathOrUri, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                path = uri.LocalPath;
            }

            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);

            var text = File.ReadAllText(path);
            var doc = new DocumentReference(path, Path.GetFileName(path), "text/markdown");
            var result = new ExtractionResult(doc, text);
            return Task.FromResult(result);
        }
    }
}
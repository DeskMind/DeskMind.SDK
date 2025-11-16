using DeskMind.Rag.Abstractions;
using DeskMind.Rag.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using UglyToad.PdfPig;

namespace DeskMind.Rag.Processing.ContentExtractors
{
    public sealed class PdfExtractor : IContentExtractor
    {
        public bool CanHandle(string pathOrUri)
        {
            if (Uri.TryCreate(pathOrUri, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                return Path.GetExtension(uri.LocalPath).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
            }

            if (File.Exists(pathOrUri))
            {
                return Path.GetExtension(pathOrUri).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
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

            var sb = new StringBuilder();
            var pages = new List<string>();

            using (var doc = PdfDocument.Open(path))
            {
                foreach (var page in doc.GetPages())
                {
                    var pText = page.Text;
                    pText = Regex.Replace(pText, @"[ \t]+\n", "\n");
                    pText = Regex.Replace(pText, @"\n{3,}", "\n\n");
                    pages.Add(pText.Trim());
                    sb.AppendLine(pText);
                    sb.AppendLine();
                }
            }

            var text = sb.ToString().Trim();
            var docRef = new DocumentReference(path, Path.GetFileName(path), "application/pdf");
            var result = new ExtractionResult(docRef, text, pages);
            return Task.FromResult(result);
        }
    }
}
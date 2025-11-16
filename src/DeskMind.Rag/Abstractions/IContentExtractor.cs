using DeskMind.Rag.Models;

using System.Threading;
using System.Threading.Tasks;

namespace DeskMind.Rag.Abstractions
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Extraction & Splitting
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts plain text from a given source. Implementations may handle TXT, MD, PDF, DOCX, HTML, etc.
    /// </summary>
    public interface IContentExtractor
    {
        /// <summary>
        /// Returns true if this extractor can handle <paramref name="pathOrUri"/>.
        /// </summary>
        bool CanHandle(string pathOrUri);

        /// <summary>
        /// Extract text and optional page map from the source.
        /// </summary>
        /// <exception cref="FileNotFoundException">If the target does not exist (for file paths).</exception>
        /// <exception cref="InvalidDataException">If the source is detected but unreadable/corrupt.</exception>
        Task<ExtractionResult> ExtractAsync(string pathOrUri, CancellationToken ct = default);
    }
}
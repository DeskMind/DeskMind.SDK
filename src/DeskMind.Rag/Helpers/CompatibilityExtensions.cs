using System;
using System.Collections.Generic;
using System.Text;

namespace DeskMind.Rag.Helpers
{
    /// <summary>
    /// Compatibility extensions for .NET Standard builds.
    /// Provides string chunking and byte-to-hex conversion found in .NET 6+.
    /// </summary>
    public static class CompatibilityExtensions
    {
        /// <summary>
        /// Splits <paramref name="text"/> into non-empty segments either:
        /// - fixed-size chunks if <paramref name="separator"/> is null or empty
        /// - by a multi-character separator (e.g. "\n\n") otherwise.
        ///
        /// No recursion. No infinite loops. Works on .NET Standard.
        /// </summary>
        public static string[] SplitIntoSegments(this string text, string? separator, int maxChunkSize)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<string>();

            if (maxChunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxChunkSize));

            var segments = new List<string>();

            if (string.IsNullOrEmpty(separator))
            {
                // Fixed-length chunks
                int total = text.Length;
                for (int i = 0; i < total; i += maxChunkSize)
                {
                    int len = Math.Min(maxChunkSize, total - i);
                    var part = text.Substring(i, len).Trim();
                    if (part.Length > 0)
                        segments.Add(part);
                }

                return segments.ToArray();
            }

            // Split by multi-character separator token, e.g. "\n\n"
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string token = separator;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            int tokenLen = token?.Length ?? 0;
            int start = 0;

            while (true)
            {
                int idx = text.IndexOf(token, start, StringComparison.Ordinal);
                if (idx < 0)
                {
                    // Tail
                    if (start < text.Length)
                    {
                        var tail = text.Substring(start).Trim();
                        if (tail.Length > 0)
                            segments.Add(tail);
                    }
                    break;
                }

                if (idx > start)
                {
                    var part = text.Substring(start, idx - start).Trim();
                    if (part.Length > 0)
                        segments.Add(part);
                }

                // Jump past the separator
                start = idx + tokenLen;
            }

            return segments.ToArray();
        }

        /// <summary>
        /// Converts a byte array to a hex string using uppercase hex.
        /// Equivalent to Convert.ToHexString in .NET 5+.
        /// </summary>
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var sb = new StringBuilder(bytes.Length * 2);

            foreach (var b in bytes)
            {
                sb.AppendFormat("{0:X2}", b);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts ReadOnlySpan&lt;byte&gt; to hex string (optional helper).
        /// </summary>
        public static string ToHexString(this ReadOnlySpan<byte> bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.AppendFormat("{0:X2}", bytes[i]);
            }
            return sb.ToString();
        }

        // <summary>
        /// Split a string by a multi-character separator safely.
        /// - Never recursive
        /// - Never produces empty segments
        /// - Never infinite loops (even on repeated separators)
        /// - Works on .NET Standard
        /// </summary>
        public static IEnumerable<string> SplitByToken(this string text, string token)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            if (string.IsNullOrEmpty(token))
            {
                yield return text;
                yield break;
            }

            int start = 0;
            int index;

            while ((index = text.IndexOf(token, start, StringComparison.Ordinal)) >= 0)
            {
                if (index > start)
                {
                    string chunk = text.Substring(start, index - start).Trim();
                    if (chunk.Length > 0)
                        yield return chunk;
                }

                // Jump past the separator
                start = index + token.Length;
            }

            // Last tail chunk
            if (start < text.Length)
            {
                string last = text.Substring(start).Trim();
                if (last.Length > 0)
                    yield return last;
            }
        }
    }
}
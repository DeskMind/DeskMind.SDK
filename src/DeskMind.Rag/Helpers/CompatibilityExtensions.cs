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
        /// Splits a string into chunks of the specified size.
        /// Equivalent to .NET 8's "string.Chunk(int)" method.
        /// </summary>
        public static IEnumerable<char[]> ChunkIt(this string source, int chunkSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize));

            for (int i = 0; i < source.Length; i += chunkSize)
            {
                var size = Math.Min(chunkSize, source.Length - i);
                var buffer = new char[size];
                source.CopyTo(i, buffer, 0, size);
                yield return buffer;
            }
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
    }
}
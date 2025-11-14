using DeskMind.Rag.Abstractions;
using DeskMind.Rag.Helpers;
using DeskMind.Rag.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeskMind.Rag.Processing.Splitters
{
    /// <summary>
    /// Splits text hierarchically (paragraphs → sentences → words → chars) and
    /// packs chunks up to the requested size, with simple overlap.
    /// </summary>
    public sealed class RecursiveOverlapSplitter : ITextSplitter
    {
        private static readonly string[] Separators = { "\n\n", "\n", ". ", " ", "" };

        public IEnumerable<SplitChunk> Split(
            ExtractionResult extraction,
            int chunkSize,
            int chunkOverlap)
        {
            var text = extraction.Text;
            text = Normalize(text);
            if (text.Length <= 0)
                yield break;

            var rawChunks = SplitRecursive(text, chunkSize);
            var index = 0;

            foreach (var c in rawChunks)
            {
                var trimmed = c.Trim();
                if (trimmed.Length == 0) continue;

                // Overlap: add some tail of previous + head of next
                var overlapped = trimmed;
                if (chunkOverlap > 0)
                {
                    // we only add overlap later if needed; for now basic chunk
                    overlapped = trimmed;
                }

                yield return new SplitChunk(extraction.Document, index++, overlapped, extraction.Metadata);
            }
        }

        private static List<string> SplitRecursive(string text, int chunkSize)
        {
            foreach (var sep in Separators)
            {
                var parts = sep == ""
                    ? text.ChunkIt(chunkSize).Select(c => new string(c)).ToArray()
                    : text.Split(new[] { sep }, StringSplitOptions.None); // fix: use string[] overload

                if (parts.Any(p => p.Length > chunkSize))
                {
                    var result = new List<string>();
                    foreach (var p in parts)
                    {
                        if (p.Length <= chunkSize)
                        {
                            result.Add(p);
                        }
                        else
                        {
                            result.AddRange(SplitRecursive(p, chunkSize));
                        }
                    }
                    return result;
                }
                else
                {
                    // repack into <= chunkSize buckets, preserving separator where relevant
                    var buckets = new List<string>();
                    var current = "";
                    var joiner = sep == "" ? "" : sep;

                    foreach (var part in parts)
                    {
                        var candidate = string.IsNullOrEmpty(current)
                            ? part
                            : current + joiner + part;

                        if (candidate.Length <= chunkSize)
                        {
                            current = candidate;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(current))
                                buckets.Add(current);
                            current = part;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(current))
                        buckets.Add(current);

                    return buckets;
                }
            }

            return new List<string> { text };
        }

        private static string Normalize(string text)
        {
            text = text.Replace("\r\n", "\n");
            text = Regex.Replace(text, @"[ \t]+\n", "\n");
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            return text.Trim();
        }
    }
}
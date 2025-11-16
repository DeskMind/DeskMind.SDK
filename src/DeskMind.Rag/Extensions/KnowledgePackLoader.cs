using DeskMind.Rag.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DeskMind.Rag.Extensions
{
    /// <summary>
    /// Helpers for loading embedded knowledge resources from plugin assemblies.
    /// </summary>
    public static class KnowledgePackLoader
    {
        /// <summary>
        /// Enumerate embedded resources under a given namespace prefix (e.g. "MyPlugin.Resources.Knowledge.")
        /// and create <see cref="DocumentReference"/> + text pairs.
        /// </summary>
        public static IEnumerable<(DocumentReference Document, string Text)> LoadEmbeddedKnowledge(
            Assembly assembly,
            string resourcePrefix,
            string? contentType = "text/markdown")
        {
            var names = assembly
                .GetManifestResourceNames()
                .Where(n => n.StartsWith(resourcePrefix, StringComparison.Ordinal))
                .ToArray();

            foreach (var name in names)
            {
                using var stream = assembly.GetManifestResourceStream(name);
                if (stream is null) continue;

                using var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();

                var fileName = name.Substring(resourcePrefix.Length).Trim('.');
                var displayName = Path.GetFileNameWithoutExtension(fileName);

                var doc = new DocumentReference(
                    Key: $"{assembly.GetName().Name}://{name}",
                    DisplayName: displayName,
                    ContentType: contentType);

                yield return (doc, text);
            }
        }
    }
}
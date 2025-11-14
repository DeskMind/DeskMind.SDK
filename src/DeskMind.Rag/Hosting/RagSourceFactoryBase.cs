using DeskMind.Core.Plugins;
using DeskMind.Core.Security;
using DeskMind.Rag.Abstractions;
using DeskMind.Rag.Models;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

namespace DeskMind.Rag.Hosting
{
    /// <summary>
    /// Base for discoverable RAG source factories (one per type/assembly).
    /// Mirrors your SecurePluginFactoryBase lifecycle.
    /// </summary>
    public abstract class RagSourceFactoryBase
    {
        protected RagSourceFactoryBase(RagSourceMetadata metadata) => Metadata = metadata;

        public RagSourceMetadata Metadata { get; }
        public bool IsEnabled { get; protected set; } = true;
        public ILogger? Logger { get; set; }

        public List<PluginConfig> Configurations { get; set; } = new();

        public abstract void LoadConfiguration();

        public abstract void SaveConfiguration();

        public abstract T GetConfigValue<T>(string name, T @default);

        /// <summary>Environment/runtime readiness (DB reachable, path exists, etc.).</summary>
        public abstract bool IsAvailable();

        /// <summary>Apply policy/user scope to set IsEnabled.</summary>
        public abstract void UpdateSecurityState(SecurityPolicy policy, string currentUserId);

        /// <summary>Create a live IRagSource instance using DI.</summary>
        public abstract IRagSource? CreateSource(IServiceProvider services);

        /// <summary>Optional cleanup when the source is removed.</summary>
        public virtual void OnRemoved() { }
    }
}
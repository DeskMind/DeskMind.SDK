using DeskMind.Core.Plugins;
using DeskMind.Core.Security;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

namespace DeskMind.Core.RAG
{
    /// <summary>
    /// Base class for RAG source factories discovered via reflection (like SecurePluginFactoryBase).
    /// One instance per assembly/type; it controls enablement, config, and builds IRagSource.
    /// </summary>
    public abstract class RagSourceFactoryBase
    {
        protected RagSourceFactoryBase(RagSourceMetadata metadata) => Metadata = metadata;

        public RagSourceMetadata Metadata { get; }
        public bool IsEnabled { get; protected set; } = true;
        public ILogger? Logger { get; set; }

        /// <summary>Arbitrary persisted config (name, type, serialized value) like your plugins.</summary>
        public List<PluginConfig> Configurations { get; set; } = new();

        /// <summary>Load persisted configuration (file/registry/db). Called by the registry before Create().</summary>
        public abstract void LoadConfiguration();

        /// <summary>Save current configuration state.</summary>
        public abstract void SaveConfiguration();

        /// <summary>Return a value from the config bag with a default.</summary>
        public abstract T GetConfigValue<T>(string name, T @default);

        /// <summary>Check runtime availability (e.g., DB reachable, file path writable).</summary>
        public abstract bool IsAvailable();

        /// <summary>Apply security policy / user visibility and set IsEnabled accordingly.</summary>
        public abstract void UpdateSecurityState(SecurityPolicy policy, string currentUserId);

        /// <summary>Create the RAG source when enabled+available. Return null to skip.</summary>
        public abstract IRagSource? CreateSource(IServiceProvider services);

        /// <summary>Optional cleanup when disabling/removing a source from the hub.</summary>
        public virtual void RemoveFromHub(IRagHub hub) { /* no-op */ }
    }
}
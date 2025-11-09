using Microsoft.SemanticKernel;

using System.Collections.Generic;

namespace DeskMind.Core.Plugins
{
    public interface IPluginFactory
    {
        bool IsEnabled { get; }

        PluginMetadata Metadata { get; }

        bool IsAvailable();

        KernelPlugin? CreatePlugin();

        /// <summary>
        /// Load persisted configuration for the plugin.
        /// </summary>
        void LoadConfiguration();

        /// <summary>
        /// Save current configuration state for persistence.
        /// </summary>
        void SaveConfiguration();

        T GetConfigValue<T>(string name, T defaultValue);

        /// <summary>
        /// Plugin-specific configuration stored as a list of tuples:
        /// Name, DataType, SerializedValue
        /// </summary>
        List<PluginConfig> Configurations { get; set; }

        //public virtual IPluginUIFactory? CreateUIFactory() => null;
    }
}
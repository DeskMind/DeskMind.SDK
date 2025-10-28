using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DeskMind.Core.Plugins
{
    public abstract class SecurePluginFactoryBase : IPluginFactory
    {
        private KernelPlugin? _plugin;
        protected bool _isEnabled = false;

        public abstract PluginMetadata Metadata { get; }
        public bool IsEnabled => _isEnabled;

        public virtual bool IsAvailable() => true;

        public KernelPlugin? CreatePlugin()
        {
            if (!IsEnabled || !IsAvailable())
                return null;

            _plugin = CreatePluginCore();
            return _plugin;
        }

        protected abstract KernelPlugin CreatePluginCore();

        public ILogger? Logger { get; set; }

        public void UpdateSecurityState(Security.SecurityPolicy policy, string userId)
        {
            var requiredRoles = Metadata.RequiredRoles;
            var prev = _isEnabled;
            _isEnabled = requiredRoles == null || requiredRoles.Length == 0 ||
                       policy.Approve(userId, requiredRoles);

            if (Logger != null)
            {
                Logger.LogTrace("Factory {Factory} security updated. WasEnabled={Was}, NowEnabled={Now}", Metadata?.Name ?? GetType().FullName, prev, _isEnabled);
            }
        }

        public void RemoveFromKernel(Kernel kernel)
        {
            if (_plugin != null)
            {
                kernel.Plugins.Remove(_plugin);
                if (Logger != null)
                {
                    Logger.LogTrace("Removed plugin {Plugin} from kernel", Metadata?.Name ?? GetType().FullName);
                }
                _plugin = null;
            }
        }

        #region Plugin Configuration

        // <summary>
        /// Configuration stored as tuples
        /// </summary>
        public List<PluginConfig> Configurations { get; set; } = new();

        protected string GetConfigFilePath()
        {
            var fileName = $"{Metadata.Name.Replace(" ", "_")}.config.json";
            var folder = AppContext.BaseDirectory;
            return Path.Combine(folder, "config", "plugin", fileName);
        }

        public virtual void LoadConfiguration()
        {
            var path = GetConfigFilePath();
            //if (!File.Exists(path)) return;

            try
            {
                var json = File.ReadAllText(path);
                Configurations = JsonSerializer.Deserialize<List<PluginConfig>>(json)
                                 ?? new List<PluginConfig>();
            }
            catch (Exception ex)
            {
                Configurations = new List<PluginConfig>();
                Logger?.LogWarning(ex, "Failed to load configuration for {Factory}", Metadata?.Name ?? GetType().FullName);
            }
            finally
            {
                if (Configurations.Count == 0)
                {
                    Configurations = GetDefaultConfigurations();
                    SaveConfiguration();
                }
            }
        }

        public virtual void SaveConfiguration()
        {
            var path = GetConfigFilePath();
            // Create directory if not exists
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }
            var json = JsonSerializer.Serialize(Configurations);
            File.WriteAllText(path, json);
            Logger?.LogInformation("Saved configuration for {Factory}", Metadata?.Name ?? GetType().FullName);
        }

        protected virtual List<PluginConfig> GetDefaultConfigurations()
        {
            // Base has no defaults, plugin factories can override
            return new List<PluginConfig>();
        }

        public object? GetConfigValue(string name)
        {
            var entry = Configurations.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (entry == default) return null;

            var converter = TypeDescriptor.GetConverter(Type.GetType(entry.DataType) ?? typeof(string));
            if (converter != null && converter.IsValid(entry.SerializedValue))
            {
                return converter.ConvertFromInvariantString(entry.SerializedValue);
            }

            return entry.SerializedValue; // fallback as string
        }

        public T GetConfigValue<T>(string name, T defaultValue)
        {
            var entry = Configurations.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (entry == default) return defaultValue;

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null && converter.IsValid(entry.SerializedValue))
                {
                    return (T)converter.ConvertFromInvariantString(entry.SerializedValue);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, "Failed to convert configuration value {Name} for {Factory}", name, Metadata?.Name ?? GetType().FullName);
            }

            return defaultValue;
        }

        #endregion Plugin Configuration
    }
}
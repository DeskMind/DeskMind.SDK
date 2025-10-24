using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DeskMind.Core.Plugins;
using DeskMind.Core.Security;

using Microsoft.SemanticKernel;

namespace DeskMind.Core.Tools
{
    public class ToolRegistry
    {
        private readonly Dictionary<Assembly, List<SecurePluginFactoryBase>> _pluginFactories = new();
        private readonly SecurityPolicy _securityPolicy;
        private readonly Kernel _kernel;
        private string? _currentUserId;

        public ToolRegistry(Kernel kernel, SecurityPolicy securityPolicy)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _securityPolicy = securityPolicy ?? throw new ArgumentNullException(nameof(securityPolicy));
        }

        public void RegisterAssembly(Assembly assembly)
        {
            if (!_pluginFactories.ContainsKey(assembly))
            {
                var factories = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && typeof(SecurePluginFactoryBase).IsAssignableFrom(t))
                    .Select(t => (SecurePluginFactoryBase)Activator.CreateInstance(t))
                    .ToList();

                if (factories.Count > 0)
                    _pluginFactories[assembly] = factories;

                // If we have a current user, update security state for new factories
                //if (!string.IsNullOrEmpty(_currentUserId))
                {
                    foreach (var factory in factories)
                    {
                        factory.LoadConfiguration();

                        // Ensure userId is not null
                        var userId = _currentUserId ?? string.Empty;
                        factory.UpdateSecurityState(_securityPolicy, userId);
                        if (factory.IsEnabled && factory.IsAvailable())
                        {
                            var plugin = factory.CreatePlugin();
                            if (plugin != null)
                            {
                                _kernel.Plugins.Add(plugin);
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<Assembly> GetPluginAssemblies()
        {
            return _pluginFactories.Keys;
        }

        public IEnumerable<SecurePluginFactoryBase> GetFactories(Assembly assembly)
        {
            return _pluginFactories.TryGetValue(assembly, out var factories) ? factories : Enumerable.Empty<SecurePluginFactoryBase>();
        }

        public void UpdateSecurityState(string userId)
        {
            _currentUserId = userId ?? string.Empty;
            foreach (var factories in _pluginFactories.Values)
            {
                foreach (var factory in factories)
                {
                    // Ensure configuration is always up-to-date
                    factory.LoadConfiguration();

                    bool wasEnabled = factory.IsEnabled;
                    factory.UpdateSecurityState(_securityPolicy, _currentUserId);

                    if (wasEnabled && !factory.IsEnabled)
                    {
                        factory.RemoveFromKernel(_kernel);
                    }
                    else if (!wasEnabled && factory.IsEnabled && factory.IsAvailable())
                    {
                        var plugin = factory.CreatePlugin();
                        if (plugin != null)
                        {
                            _kernel.Plugins.Add(plugin);
                        }
                    }
                }
            }
        }

        public void ResetSecurity()
        {
            _currentUserId = null;
            foreach (var factories in _pluginFactories.Values)
            {
                foreach (var factory in factories)
                {
                    // Ensure configuration is always up-to-date
                    factory.LoadConfiguration();

                    bool wasEnabled = factory.IsEnabled;
                    factory.UpdateSecurityState(_securityPolicy, string.Empty);

                    // Handle kernel plugin state changes
                    if (wasEnabled && !factory.IsEnabled)
                    {
                        factory.RemoveFromKernel(_kernel);
                    }
                    else if (!wasEnabled && factory.IsEnabled && factory.IsAvailable())
                    {
                        var plugin = factory.CreatePlugin();
                        if (plugin != null)
                        {
                            _kernel.Plugins.Add(plugin);
                        }
                    }
                }
            }
        }
    }
}


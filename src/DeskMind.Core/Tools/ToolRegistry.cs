using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DeskMind.Core.Plugins;
using DeskMind.Core.Security;

using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;

namespace DeskMind.Core.Tools
{
    public class ToolRegistry
    {
        private readonly Dictionary<Assembly, List<SecurePluginFactoryBase>> _pluginFactories = new();
        private readonly SecurityPolicy _securityPolicy;
        private readonly Kernel _kernel;
        private readonly ILogger<ToolRegistry> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private string? _currentUserId;
        private readonly List<DefaultPrompt> _defaultPrompts = new();

        #region Constructors

        public ToolRegistry(Kernel kernel, SecurityPolicy securityPolicy, ILogger<ToolRegistry> logger, ILoggerFactory loggerFactory)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _securityPolicy = securityPolicy ?? throw new ArgumentNullException(nameof(securityPolicy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        #endregion Constructors

        public IEnumerable<DefaultPrompt> DefaultPrompts => _defaultPrompts;

        public void RegisterAssembly(Assembly assembly)
        {
            if (!_pluginFactories.ContainsKey(assembly))
            {
                var factories = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && typeof(SecurePluginFactoryBase).IsAssignableFrom(t))
                    .Select(t => (SecurePluginFactoryBase)Activator.CreateInstance(t)!) // ! to assert it's not null
                    .ToList();

                if (factories.Count > 0)
                    _pluginFactories[assembly] = factories;

                if (factories.Count > 0)
                    _logger.LogTrace("Found {Count} plugin factories in assembly {Assembly}", factories.Count, assembly.FullName);

                // If we have a current user, update security state for new factories
                //if (!string.IsNullOrEmpty(_currentUserId))
                {
                    foreach (var factory in factories)
                    {
                        try
                        {
                            // assign a logger to the factory so it can log its own actions
                            factory.Logger = _loggerFactory.CreateLogger(factory.GetType());

                            factory.LoadConfiguration();

                            // Collect default prompts
                            if (factory.Metadata?.DefaultPrompts != null)
                                foreach (var item in factory.Metadata.DefaultPrompts)
                                {
                                    _defaultPrompts.Add(item);
                                }
                            // Ensure userId is not null
                            var userId = _currentUserId ?? string.Empty;
                            _logger.LogDebug("Updating security state for factory {Factory} for user {User}", factory.Metadata?.Name ?? factory.GetType().FullName, userId);
                            factory.UpdateSecurityState(_securityPolicy, userId);
                            if (factory.IsEnabled && factory.IsAvailable())
                            {
                                var plugin = factory.CreatePlugin();
                                if (plugin != null)
                                {
                                    _kernel.Plugins.Add(plugin);
                                    _logger.LogInformation("Plugin {Plugin} added to kernel", factory.Metadata?.Name ?? factory.GetType().FullName);
                                }
                            }
                            else
                            {
                                _logger.LogDebug("Factory {Factory} is not enabled or not available", factory.Metadata?.Name ?? factory.GetType().FullName);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error initializing plugin factory {Factory}", factory.GetType().FullName);
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
            _logger.LogInformation("Updating security state for user {User}", _currentUserId);
            foreach (var factories in _pluginFactories.Values)
            {
                foreach (var factory in factories)
                {
                    try
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
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating security state for factory {Factory}", factory.GetType().FullName);
                    }
                }
            }
        }

        public void ResetSecurity()
        {
            _currentUserId = null;
            _logger.LogInformation("Resetting security state");
            foreach (var factories in _pluginFactories.Values)
            {
                foreach (var factory in factories)
                {
                    try
                    {
                        // Ensure configuration is always up-to-date
                        factory.LoadConfiguration();

                        bool wasEnabled = factory.IsEnabled;
                        factory.UpdateSecurityState(_securityPolicy, string.Empty);

                        // Handle kernel plugin state changes
                        if (wasEnabled && !factory.IsEnabled)
                        {
                            factory.RemoveFromKernel(_kernel);
                            _logger.LogInformation("Plugin {Plugin} disabled and removed from kernel", factory.Metadata?.Name ?? factory.GetType().FullName);
                        }
                        else if (!wasEnabled && factory.IsEnabled && factory.IsAvailable())
                        {
                            var plugin = factory.CreatePlugin();
                            if (plugin != null)
                            {
                                _kernel.Plugins.Add(plugin);
                                _logger.LogInformation("Plugin {Plugin} enabled and added to kernel", factory.Metadata?.Name ?? factory.GetType().FullName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error resetting security for factory {Factory}", factory.GetType().FullName);
                    }
                }
            }
        }
    }
}
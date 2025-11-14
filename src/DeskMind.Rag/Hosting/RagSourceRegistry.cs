using DeskMind.Core.Security;
using DeskMind.Rag.Abstractions;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeskMind.Rag.Hosting
{
    /// <summary>
    /// Finds RagSourceFactoryBase in assemblies, enforces policy, creates sources and
    /// registers them into IRagHub. Mirrors your ToolRegistry pattern.
    /// </summary>
    public sealed class RagSourceRegistry
    {
        private readonly Dictionary<Assembly, List<RagSourceFactoryBase>> _factories = new();
        private readonly IRagHub _hub;
        private readonly SecurityPolicy _security;
        private readonly ILogger<RagSourceRegistry> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceProvider _services;
        private string _currentUserId = string.Empty;

        public RagSourceRegistry(
            IRagHub hub,
            SecurityPolicy security,
            ILogger<RagSourceRegistry> logger,
            ILoggerFactory loggerFactory,
            IServiceProvider services)
        {
            _hub = hub;
            _security = security;
            _logger = logger;
            _loggerFactory = loggerFactory;
            _services = services;
        }

        public IEnumerable<Assembly> GetAssemblies() => _factories.Keys;

        public IEnumerable<RagSourceFactoryBase> GetFactories(Assembly a) =>
            _factories.TryGetValue(a, out var list) ? list : Enumerable.Empty<RagSourceFactoryBase>();

        public void RegisterAssembly(Assembly assembly)
        {
            if (_factories.ContainsKey(assembly)) return;

            var factories = assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(RagSourceFactoryBase).IsAssignableFrom(t))
                .Select(t => (RagSourceFactoryBase)Activator.CreateInstance(t)!)
                .ToList();

            if (factories.Count == 0) return;

            _factories[assembly] = factories;
            _logger.LogTrace("Discovered {Count} RAG factories in {Assembly}", factories.Count, assembly.FullName);

            foreach (var f in factories)
            {
                try
                {
                    f.Logger = _loggerFactory.CreateLogger(f.GetType());
                    f.LoadConfiguration();

                    f.UpdateSecurityState(_security, _currentUserId);

                    if (f.IsEnabled && f.IsAvailable())
                    {
                        var src = f.CreateSource(_services);
                        if (src != null)
                        {
                            _hub.Register(src.Name, src);
                            _logger.LogInformation("Registered RAG source '{Name}'", src.Name);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("RAG factory {Factory} not enabled/available", f.GetType().Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating RAG source from {Factory}", f.GetType().FullName);
                }
            }
        }

        public void UpdateSecurityState(string userId)
        {
            _currentUserId = userId ?? string.Empty;
            _logger.LogInformation("RAG: updating security for user {User}", _currentUserId);

            foreach (var list in _factories.Values)
                foreach (var f in list)
                {
                    try
                    {
                        f.LoadConfiguration();
                        var wasEnabled = f.IsEnabled;

                        f.UpdateSecurityState(_security, _currentUserId);

                        if (wasEnabled && !f.IsEnabled)
                        {
                            if (_hub.Sources.Contains(f.Metadata.Name))
                            {
                                _hub.Remove(f.Metadata.Name);
                                f.OnRemoved();
                                _logger.LogInformation("RAG source '{Name}' disabled/removed", f.Metadata.Name);
                            }
                        }
                        else if (!wasEnabled && f.IsEnabled && f.IsAvailable())
                        {
                            var src = f.CreateSource(_services);
                            if (src != null)
                            {
                                _hub.Register(src.Name, src);
                                _logger.LogInformation("RAG source '{Name}' enabled/registered", src.Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating RAG security for {Factory}", f.GetType().FullName);
                    }
                }
        }

        public void ResetSecurity()
        {
            UpdateSecurityState(string.Empty);
        }
    }
}
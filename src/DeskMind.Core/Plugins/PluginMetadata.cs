using System;

namespace DeskMind.Core.Plugins
{
    public class PluginMetadata
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public string Description { get; set; } = string.Empty;
        public string[] RequiredRoles { get; set; } = Array.Empty<string>();
        public string[] Dependencies { get; set; } = Array.Empty<string>();
    }
}


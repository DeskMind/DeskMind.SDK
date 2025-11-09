using DeskMind.Core.Plugins;

using System;

namespace DeskMind.Core.UI
{
    public interface IPluginUIProvider
    {
        string PluginName { get; }

        object PluginIcon { get; }

        Type TargetPageType { get; }

        /// <summary>
        /// Creates a UI control for the plugin.
        /// For WPF: returns UserControl
        /// For Avalonia: returns Control
        /// For WinForms: returns Control
        /// </summary>
        object CreateControl(SecurePluginFactoryBase factory);
    }
}
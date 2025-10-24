using DeskMind.Core.Plugins;

namespace DeskMind.Core.UI
{
    public interface IPluginUIProvider
    {
        string PluginName { get; }

        /// <summary>
        /// Creates a UI control for the plugin.
        /// For WPF: returns UserControl
        /// For Avalonia: returns Control
        /// For WinForms: returns Control
        /// </summary>
        object CreateControl(SecurePluginFactoryBase factory);
    }
}


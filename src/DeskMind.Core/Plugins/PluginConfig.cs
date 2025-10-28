using CommunityToolkit.Mvvm.ComponentModel;

namespace DeskMind.Core.Plugins
{
    public partial class PluginConfig : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _dataType = typeof(string).FullName;

        [ObservableProperty]
        private string _serializedValue = string.Empty;

        public PluginConfig()
        {
        }

        public PluginConfig(string name, string dataType, string serializedValue)
        {
            Name = name;
            DataType = dataType;
            SerializedValue = serializedValue;
        }
    }
}
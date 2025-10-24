namespace DeskMind.Core.Plugins
{
    public class PluginConfig
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string SerializedValue { get; set; }

        public PluginConfig(string name, string dataType, string serializedValue)
        {
            Name = name;
            DataType = dataType;
            SerializedValue = serializedValue;
        }
    }
}


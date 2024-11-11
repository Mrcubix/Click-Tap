using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.Lib.Entities.Serializable
{
    public class SerializableBinding : SerializablePluginSettings
    {
        public SerializableBinding() : base() { }

        public SerializableBinding(string value, int identifier) : base(value, identifier) { }

        public SerializableBinding(SerializablePluginSettings pluginProperty) : base(pluginProperty.Value!, pluginProperty.Identifier) { }
    }
}
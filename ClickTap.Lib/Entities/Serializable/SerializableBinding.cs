using Newtonsoft.Json;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.Lib.Entities.Serializable
{
    public class SerializableBinding
    {
        public SerializableBinding()
        {
            PluginProperty = null;
        }

        public SerializableBinding(SerializablePluginSettings? pluginProperty)
        {
            PluginProperty = pluginProperty;
        }

        [JsonProperty("Store")]
        public SerializablePluginSettings? PluginProperty { get; set; }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenTabletDriver.External.Common.Serializables.Properties;

namespace ClickTap.Lib.Entities.Serializable.Bindings
{
    public class SerializableThresholdBinding : SerializableBinding
    {
        public SerializableThresholdBinding() : base()
        {
            IsThresholdBinding = true;
        }

        public SerializableThresholdBinding(string? pluginName, string? fullName, int identifier, IEnumerable<SerializableProperty> properties)
        {
            PluginName = pluginName;
            FullName = fullName;
            Identifier = identifier;
            Properties = new(properties);
        }
    }
}
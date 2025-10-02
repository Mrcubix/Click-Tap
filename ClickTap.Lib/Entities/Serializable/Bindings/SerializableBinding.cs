using System.Collections.Generic;
using System.Text.Json.Serialization;
using OpenTabletDriver.External.Common.Enums;
using OpenTabletDriver.External.Common.Serializables;
using OpenTabletDriver.External.Common.Serializables.Properties;

namespace ClickTap.Lib.Entities.Serializable
{
    public class SerializableBinding : SerializablePlugin
    {
        [JsonConstructor]
        public SerializableBinding() : base() 
        {
            Type = PluginType.Binding;
        }

        public SerializableBinding(string? pluginName, string? fullName, int identifier, IEnumerable<SerializableProperty> properties)
        {
            PluginName = pluginName;
            FullName = fullName;
            Identifier = identifier;
            Properties = new(properties);
        }
    
        public bool IsThresholdBinding { get; init; } = false;
    }
}
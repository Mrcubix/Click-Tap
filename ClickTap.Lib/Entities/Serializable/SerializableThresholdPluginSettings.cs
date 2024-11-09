using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.Lib.Entities.Serializable
{
    public class SerializableThresholdPluginSettings : SerializablePluginSettings
    {
        public SerializableThresholdPluginSettings()
        {
            Value = null!;
            Identifier = -1;
            ActivationThreshold = 0;
        }

        public SerializableThresholdPluginSettings(string value, int identifier, float activationThreshold)
        {
            Value = value;
            Identifier = identifier;
            ActivationThreshold = activationThreshold;
        }

        public float ActivationThreshold { set; get; }
    }
}
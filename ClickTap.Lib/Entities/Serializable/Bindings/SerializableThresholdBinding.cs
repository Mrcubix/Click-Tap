namespace ClickTap.Lib.Entities.Serializable.Bindings
{
    public class SerializableThresholdBinding : SerializableBinding
    {
        public SerializableThresholdBinding() : base() { }

        public SerializableThresholdBinding(string value, int identifier, float activationThreshold) : base(value, identifier)
        {
            ActivationThreshold = activationThreshold;
        }

        public SerializableThresholdBinding(SerializableThresholdPluginSettings pluginProperty) : base(pluginProperty) 
        { 
            ActivationThreshold = pluginProperty.ActivationThreshold;
        }

        public float ActivationThreshold { get; set; }
    }
}
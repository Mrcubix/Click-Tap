using System.Collections.Generic;
using System.Reflection;
using ClickTap.Lib.Entities.Serializable.Bindings;

namespace ClickTap.Lib.Bindings
{
    public abstract class ThresholdBinding : Binding
    {
        public ThresholdBinding() { }

        public ThresholdBinding(float activationThreshold, SerializableThresholdBinding binding, Dictionary<int, TypeInfo> identifierToPlugin) 
            : base(binding, identifierToPlugin) { }

        public float ActivationThreshold { set; get; }

        public virtual void Invoke(float value)
        {
            bool newState = value > ActivationThreshold;

            base.Invoke(newState);
        }
    }
}
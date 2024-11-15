using System.Collections.Generic;
using System.Reflection;
using ClickTap.Lib.Entities.Serializable.Bindings;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Tablet;

namespace ClickTap.Lib.Bindings
{
    public abstract class ThresholdBinding : Binding
    {
        public ThresholdBinding() { }

        public ThresholdBinding(float activationThreshold, SerializableThresholdBinding binding, Dictionary<int, TypeInfo> identifierToPlugin) 
            : base(binding, identifierToPlugin) 
        { 
            ActivationThreshold = activationThreshold;
        }

        /// <summary>
        ///   The threshold at which the binding should be activated.<br/>
        ///   Usually represented by a slider in the UI.
        /// </summary>
        [JsonProperty]
        public float ActivationThreshold { set; get; }

        public virtual void Invoke(IDeviceReport report, float value)
        {
            bool newState = value > ActivationThreshold;

            base.Invoke(report, newState);
        }
    }
}
using System.Reflection;
using ClickTap.Lib.Bindings;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Entities.Serializable.Bindings;

namespace ClickTap.Bindings
{
    public class BulletproofBindingBuilder : BindingBuilder
    {
        public override Binding? BuildStateFromSerializable(SerializableThresholdBinding binding, Dictionary<int, TypeInfo> identifierToPlugin)
        {
            return new BulletproofBinding(binding, identifierToPlugin);
        }

        public override SerializableBinding? BuildStateToSerializable(Binding binding, Dictionary<int, TypeInfo> identifierToPlugin)
        {
            if (binding is not BulletproofBinding bulletproofBinding)
                return null;

            return bulletproofBinding.ToSerializable(identifierToPlugin);
        }

        public override ThresholdBinding? BuildThresholdFromSerializable(SerializableThresholdBinding binding, Dictionary<int, TypeInfo> identifierToPlugin)
        {
            return new BulletproofThresholdBinding(binding.ActivationThreshold, binding, identifierToPlugin);
        }

        public override SerializableThresholdBinding? BuildThresholdToSerializable(ThresholdBinding binding, Dictionary<int, TypeInfo> identifierToPlugin)
        {
            if (binding is not BulletproofThresholdBinding bulletproofThresholdBinding)
                return null;

            return bulletproofThresholdBinding.ToSerializable(identifierToPlugin) as SerializableThresholdBinding;
        }
    }
}
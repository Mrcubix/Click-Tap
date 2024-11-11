using System.Reflection;
using ClickTap.Lib.Entities.Serializable;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.External.Common.Serializables;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using ClickTap.Lib.Bindings;
using ClickTap.Lib.Entities.Serializable.Bindings;
using ClickTap.Lib.Extensions;
using OpenTabletDriver.Plugin.Attributes;

namespace ClickTap.Bindings
{
    public class BulletproofThresholdBinding : ThresholdBinding
    {
        public BulletproofThresholdBinding() { }

        public BulletproofThresholdBinding(float activationThreshold, SerializableThresholdBinding serializableThresholdBinding, 
                                           Dictionary<int, TypeInfo> identifierToPlugin, TabletReference? tablet = null, IServiceManager? provider = null)
            : base(activationThreshold, serializableThresholdBinding, identifierToPlugin) 
        {
            Tablet = tablet;
            Provider = provider;
        }

        public IServiceManager? Provider { get; set; }
        public TabletReference? Tablet { get; set; }
        public PluginSettingStore? Store { get; set; }

        public IBinding? Binding { get; protected set; }

        public override void Construct() => Binding = Store?.Construct<IBinding>(Provider, Tablet);

        public override void Press()
        {
            if (Binding is IStateBinding stateBinding)
                stateBinding.Press(null!, null!);
        }

        public override void Release()
        {
            if (Binding is IStateBinding stateBinding)
                stateBinding.Release(null!, null!);
        }

        public override SerializableBinding ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin)
        {
            var identifier = identifierToPlugin.FirstOrDefault(x => x.Value.FullName == Store?.Path);
            var value = Store?.Settings.FirstOrDefault(x => x.Property == "Property");

            return new SerializableThresholdBinding()
            {
                ActivationThreshold = ActivationThreshold,
                Identifier = identifier.Key,
                Value = value?.GetValue<string?>()
            };
        }

        public override void FromSerializable(SerializableBinding binding, Dictionary<int, TypeInfo> identifierToPlugin)
        {
            if (binding is not SerializableThresholdBinding serializableThresholdBinding)
                return;

            ActivationThreshold = serializableThresholdBinding.ActivationThreshold;

            // The plugin might not be installed anymore or isn't loaded 
            if (identifierToPlugin.TryGetValue(binding.Identifier, out var typeInfo) == false)
                return;

            var store = new PluginSettingStore(typeInfo);

            // At first, i only fetched the property with a PropertyValidatedAttribute
            var valueProperty = typeInfo?.FindPropertyWithAttribute<PropertyAttribute>();
            var validatedProperty = typeInfo?.FindPropertyWithAttribute<PropertyValidatedAttribute>();

            if (valueProperty == null || validatedProperty == null)
                return;

            // TODO: When multiple properties are supported in OpenTabletDriver.External, we need to feed all settings
            store.Settings.Single(s => s.Property == valueProperty.Name).SetValue(binding.Value);

            // Only set the store when it's valid
            Store = store;
        }
    }
}
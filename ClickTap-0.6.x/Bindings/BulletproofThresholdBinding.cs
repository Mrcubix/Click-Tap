using System.Reflection;
using ClickTap.Lib.Entities.Serializable;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using ClickTap.Lib.Bindings;
using ClickTap.Lib.Entities.Serializable.Bindings;
using ClickTap.Lib.Extensions;
using OpenTabletDriver.Plugin.Attributes;
using Newtonsoft.Json;

namespace ClickTap.Bindings
{
    public class BulletproofThresholdBinding : ThresholdBinding, IBulletproofBinding
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

        [JsonProperty]
        public PluginSettingStore? Store { get; set; }

        public IBinding? Binding { get; protected set; }

        public override void Construct() => Binding = Store?.Construct<IBinding>(Provider, Tablet);

        public override void Press(IDeviceReport report)
        {
            if (Binding is IStateBinding stateBinding)
                stateBinding.Press(Tablet, report);
        }

        public override void Release(IDeviceReport report)
        {
            if (Binding is IStateBinding stateBinding)
                stateBinding.Release(Tablet, report);
        }

        public override SerializableBinding ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin)
        {
            if (Store == null)
                return null!;

            var pluginKeyPair = identifierToPlugin.FirstOrDefault(x => x.Value == Store.GetTypeInfo());

            if (pluginKeyPair.Key == 0)
                return null!;

            var identifier = pluginKeyPair.Key;
            var plugin = pluginKeyPair.Value;
            string? value;

            if (Store.Settings.Count == 1)
                value = Store.Settings[0].GetValue<string?>();
            else
            {
                // 0.6 so smart that you have to check for 45454 properties just to get the valid keys properly
                var valueProperty = plugin.FindPropertyWithAttribute<PropertyAttribute>();
                var validatedProperty = plugin.FindPropertyWithAttribute<PropertyValidatedAttribute>();

                if (valueProperty == null || validatedProperty == null)
                    return null!;

                // surely they are the same property
                if (valueProperty != validatedProperty)
                    return null!;

                value = Store.Settings.FirstOrDefault(x => x.Property == valueProperty.Name)?.GetValue<string?>();
            }

            return new SerializableThresholdBinding()
            {
                ActivationThreshold = ActivationThreshold,
                Identifier = identifier,
                Value = value
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
            // 0.6 so smart that you have to check for 45454 properties just to set the key properly
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
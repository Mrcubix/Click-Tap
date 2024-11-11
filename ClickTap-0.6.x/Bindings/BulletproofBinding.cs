using System.Reflection;
using ClickTap.Lib.Entities.Serializable;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using ClickTap.Lib.Bindings;
using OpenTabletDriver.Plugin.Attributes;
using ClickTap.Lib.Extensions;
using Newtonsoft.Json;

namespace ClickTap.Bindings
{
    public class BulletproofBinding : Binding, IBulletproofBinding
    {
        public BulletproofBinding() { }

        public BulletproofBinding(SerializableBinding binding, Dictionary<int, TypeInfo> identifierToPlugin,
                                  TabletReference? tablet = null, IServiceManager? provider = null)
            : base(binding, identifierToPlugin)
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
            if (Store == null)
                return null!;

            var pluginKeyPair = identifierToPlugin.FirstOrDefault(x => x.Value == Store.GetTypeInfo());

            var identifier = pluginKeyPair.Key;
            var plugin = pluginKeyPair.Value;
            string? value;

            if (Store.Settings.Count == 1)
                value = Store.Settings[0].GetValue<string?>();
            else
            {
                var valueProperty = plugin.FindPropertyWithAttribute<PropertyAttribute>();
                var validatedProperty = plugin.FindPropertyWithAttribute<PropertyValidatedAttribute>();

                if (valueProperty == null || validatedProperty == null)
                    return null!;

                // surely they are the same property
                if (valueProperty != validatedProperty)
                    return null!;

                value = Store.Settings.FirstOrDefault(x => x.Property == valueProperty.Name)?.GetValue<string?>();
            }

            return new SerializableBinding()
            {
                Identifier = identifier,
                Value = value
            };
        }

        public override void FromSerializable(SerializableBinding binding, Dictionary<int, TypeInfo> identifierToPlugin)
        {
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
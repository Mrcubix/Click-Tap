using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.Extensions
{
    public static class SerializablePluginSettingStoreExtensions
    {
        public static PluginSettingStore FromSerializable(this SerializablePluginSettingsStore store)
        {
            return new PluginSettingStore(null, true)
            {
                Path = store.FullName,
                Settings = store.Settings.FromSerializableCollection(),
            };
        }
    }
}
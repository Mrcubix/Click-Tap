using System.Collections.ObjectModel;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.Extensions
{
    public static class PluginSettingExtensions
    {
        public static PluginSetting FromSerializable(this SerializablePluginSettings serializablePluginSetting)
        {
            return new PluginSetting(serializablePluginSetting.Property, serializablePluginSetting.Value);
        }

        public static SerializablePluginSettings ToSerializable(this PluginSetting pluginSetting)
        {
            return new SerializablePluginSettings()
            {
                Property = pluginSetting.Property,
                Value = pluginSetting.Value
            };
        }

        public static ObservableCollection<PluginSetting> FromSerializableCollection(this ObservableCollection<SerializablePluginSettings> serializablePluginSettings)
        {
            return new ObservableCollection<PluginSetting>(serializablePluginSettings.Select(setting => setting.FromSerializable()));
        }
        
        public static ObservableCollection<SerializablePluginSettings> ToSerializableCollection(this ObservableCollection<PluginSetting> pluginSettings)
        {
            return new ObservableCollection<SerializablePluginSettings>(pluginSettings.Select(setting => setting.ToSerializable()));
        }
    }
}
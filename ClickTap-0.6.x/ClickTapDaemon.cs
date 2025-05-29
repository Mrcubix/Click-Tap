using System.Diagnostics;
using System.Reflection;
using ClickTap.Lib;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.External.Common.RPC;
using OpenTabletDriver.External.Common.Serializables;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using ClickTap.Entities;
using ClickTap.Bindings;
using OpenTabletDriver.External.Common.Serializables.Properties;
using ClickTap.Extensions;
using OpenTabletDriver.External.Common.Enums;

namespace ClickTap
{
    public class BulletproofClickTapDaemonBase : ClickTapDaemonBase<BulletproofBindableProfile, 
                                                                    BulletproofBinding, 
                                                                    BulletproofThresholdBinding> { }

    /// <summary>
    ///   Manages settings for each tablets as well as the RPC server.
    /// </summary>
    [PluginName(PLUGIN_NAME)]
    public class ClickTapDaemon : BulletproofClickTapDaemonBase, ITool
    {
        #region Constructors

        public ClickTapDaemon()
        {
#if DEBUG
            WaitForDebugger();
#endif
            _rpcServer ??= new RpcServer<ClickTapDaemonBase<BulletproofBindableProfile, 
                                                            BulletproofBinding, 
                                                            BulletproofThresholdBinding>>("ClickTapDaemon", this);
            Instance ??= this;
        }
#if DEBUG
        private void WaitForDebugger()
        {
            Console.WriteLine("Waiting for debugger to attach...");

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }
        }
#endif
        #endregion

        #region Methods

        protected override void SerializePlugins()
        {
            Plugins.Clear();

            foreach (var IdentifierPluginPair in IdentifierToPluginConversion)
            {
                var plugin = IdentifierPluginPair.Value;

                var store = new PluginSettingStore(plugin);

                var type = store.GetTypeInfo();

                if (type == null)
                    continue; // type doesn't exist 

                // There are situation where the name isn't specified, in which case we use the type's FullName
                var pluginName = plugin.GetCustomAttribute<PluginNameAttribute>()?.Name 
                                 ?? plugin.FullName ?? $"Plugin {IdentifierPluginPair.Key}";

                // We only support properties decoarted with the [Property] attribute
                var properties = from property in type.GetProperties() 
                                 let attrs = property.GetCustomAttributes(true)
                                 where attrs.Any(attr => attr is PropertyAttribute)
                                 select property;

                // We now need to serialized all properties
                var serializedProperties = new List<SerializableProperty>();

                try
                {
                    foreach (var property in properties)
                    {
                        var serialized = property.ToSerializable();
                        serializedProperties.Add(serialized);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(PLUGIN_NAME, $"An Error occured while serializing a property from '{pluginName}': {ex.Message}", LogLevel.Error);
                    continue;
                }

                Plugins.Add(
                    new SerializablePlugin(pluginName,
                                           plugin.FullName,
                                           IdentifierPluginPair.Key,
                                           serializedProperties)
                    {
                        Type = PluginType.Binding
                    }
                );
            }

            Log.Write(PLUGIN_NAME, $"Found {Plugins.Count} Usable Bindings Plugins.");
        }

        #endregion
    }
}
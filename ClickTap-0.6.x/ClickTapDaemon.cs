using System.Diagnostics;
using System.Reflection;
using ClickTap.Lib;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.External.Common.RPC;
using OpenTabletDriver.External.Common.Serializables;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using ClickTap.Lib.Extensions;
using ClickTap.Entities;
using ClickTap.Bindings;

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
        
        private void WaitForDebugger()
        {
            Console.WriteLine("Waiting for debugger to attach...");

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }
        }

        #endregion

        #region RPC Methods

        public override Task<List<SerializablePlugin>> GetPlugins()
        {
            Log.Write(PLUGIN_NAME, "Getting plugins...");

            List<SerializablePlugin> plugins = new();

            foreach (var IdentifierPluginPair in IdentifierToPluginConversion)
            {
                var plugin = IdentifierPluginPair.Value;

                var store = new PluginSettingStore(plugin);

                var type = store.GetTypeInfo();

                if (type == null)
                    continue; // type doesn't exist 

                var properties = type.GetProperties();

                // ALL that extra reflection bs just to get valid keys
                var property = store.GetTypeInfo()?.FindPropertyWithAttribute<PropertyValidatedAttribute>();
                var attribute = property?.GetCustomAttribute<PropertyValidatedAttribute>();

                IEnumerable<string> validKeys = attribute?.GetValue<IEnumerable<string>>(property) ?? 
                                                Array.Empty<string>();

                var serializablePlugin = new SerializablePlugin(plugin.GetCustomAttribute<PluginNameAttribute>()?.Name,
                                                                plugin.FullName,
                                                                IdentifierPluginPair.Key,
                                                                validKeys.ToArray());

                plugins.Add(serializablePlugin);
            }

            Log.Write(PLUGIN_NAME, $"Found {plugins.Count} Usable Bindings Plugins.");

            return Task.FromResult(plugins);
        }

        #endregion
    }
}
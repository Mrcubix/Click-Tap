using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ClickTap.Lib;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.External.Common.RPC;
using OpenTabletDriver.External.Common.Serializables;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using ClickTap.Lib.Converters;
using ClickTap.Lib.Extensions;

namespace TouchGestures
{
    /// <summary>
    ///   Manages settings for each tablets as well as the RPC server.
    /// </summary>
    [PluginName(PLUGIN_NAME)]
    public class ClickTapDaemon : ClickTapDaemonBase, ITool
    {
        #region Constructors

        public ClickTapDaemon()
        {
#if DEBUG
            WaitForDebugger();
#endif
            _rpcServer ??= new RpcServer<ClickTapDaemonBase>("GesturesDaemon", this);
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
            Log.Write("Gestures Daemon", "Getting plugins...");

            List<SerializablePlugin> plugins = new();

            foreach (var IdentifierPluginPair in IdentifierToPluginConversion)
            {
                var plugin = IdentifierPluginPair.Value;

                var store = new PluginSettingStore(plugin);

                // ALL that extra reflection bs just to get valid keys
                var property = store.GetTypeInfo()?.FindPropertyWithAttribute<PropertyValidatedAttribute>();

                if (property == null)
                    continue;

                var attribute = property.GetCustomAttribute<PropertyValidatedAttribute>();

                if (attribute == null)
                    continue;

                var validKeys = attribute.GetValue<IEnumerable<string>>(property);

                var serializablePlugin = new SerializablePlugin(plugin.GetCustomAttribute<PluginNameAttribute>()?.Name,
                                                                plugin.FullName,
                                                                IdentifierPluginPair.Key,
                                                                validKeys.ToArray());

                plugins.Add(serializablePlugin);
            }

            Log.Write("Gestures Daemon", $"Found {plugins.Count} Usable Bindings Plugins.");

            return Task.FromResult(plugins);
        }

        public override Task<bool> StartRecording()
        {
            return Task.FromResult(true);
        }

        public override Task<bool> StopRecording()
        {
            return Task.FromResult(true);
        }

        #endregion
    }
}
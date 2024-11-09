using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTabletDriver.External.Common.RPC;
using OpenTabletDriver.External.Common.Serializables;
using OpenTabletDriver.Plugin;
using ClickTap.Lib.Converters;
using ClickTap.Lib.Tablet;
using ClickTap.Lib.Entities.Bindable;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Bindings;

#nullable enable

namespace ClickTap.Lib
{
    public class ClickTapDaemonBase<Tprofile, TState, Tthreshold> : IDisposable
        where Tprofile : BindableProfile<TState, Tthreshold>, new()
        where TState : Binding
        where Tthreshold : ThresholdBinding
    {
        protected static RpcServer<ClickTapDaemonBase<Tprofile, TState, Tthreshold>>? _rpcServer = null!;
        public static ClickTapDaemonBase<Tprofile, TState, Tthreshold>? Instance { get; protected set; } = null!;
        public static event EventHandler? DaemonLoaded;

        #region Constants

        public const string PLUGIN_NAME = "Click & Tap Daemon";

        protected readonly string _settingsPath = Path.Combine(OpenTabletDriver.Desktop.AppInfo.Current.AppDataDirectory, "Click-Tap.json");

        protected readonly JsonSerializerSettings _serializerSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new PluginSettingStoreConverter(),
                new PluginSettingConverter()
            }
        };

        #endregion

        #region Fields

        protected List<SharedTabletReference> _tablets = new();

        #endregion

        #region Initialization

        public virtual bool Initialize()
        {
            Initialize(true);

            if (HasErrored)
            {
                Log.Write("Gestures Daemon", "Failed to Initialize, aborting early.");
                return false;
            }

            _ = Task.Run(() => _rpcServer?.MainAsync());

            Log.Write(PLUGIN_NAME, "Initialized");

            return true;
        }

        protected virtual void Initialize(bool doLoadSettings = true)
        {
            if (doLoadSettings)
                LoadSettings(_settingsPath);

            if (HasErrored)
            {
                Log.Write(PLUGIN_NAME, "Failed to load settings, aborting initialization.");
                return;
            }

            // identify plugins
            IdentifyPlugins();

            // construct bindings
            //TouchGestureSettings?.ConstructBindings();

            SettingsChanged?.Invoke(this, ClickTapSettings);

            IsReady = true;
            Ready?.Invoke(this, null!);
            DaemonLoaded?.Invoke(this, null!);
        }

        protected virtual void IdentifyPlugins()
        {
            // Obtain all IBinding plugins
            var plugins = OpenTabletDriver.Desktop.AppInfo.PluginManager.GetChildTypes<IBinding>();

            IdentifierToPluginConversion.Clear();

            foreach (var plugin in plugins)
            {
                var identifierHash = 0;

                if (plugin.FullName == null)
                    continue;

                for (var i = 0; i < plugin.FullName.Length; i++)
                    identifierHash += plugin.FullName[i];

                if (!IdentifierToPluginConversion.ContainsKey(identifierHash))
                    IdentifierToPluginConversion.Add(identifierHash, plugin);
            }
        }

        #endregion

        #region Events

        public event EventHandler? Ready;
        public event EventHandler<Settings<Tprofile, TState, Tthreshold>?>? SettingsChanged;
        public event EventHandler<IEnumerable<SharedTabletReference>>? TabletsChanged;

        protected event EventHandler<SharedTabletReference>? TabletAdded;
        protected event EventHandler<SharedTabletReference>? TabletRemoved;

        #endregion

        #region Properties

        public Dictionary<int, TypeInfo> IdentifierToPluginConversion = new();

        public Settings<Tprofile, TState, Tthreshold>? ClickTapSettings { get; protected set; } = Settings<Tprofile, TState, Tthreshold>.Default;

        public bool IsReady { get; protected set; }

        public bool HasErrored { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        ///   Gets the settings for a specific tablet.
        /// </summary>
        /// <param name="name">The name of the tablet.</param>
        /// <returns>The settings for the tablet.</returns>
        public virtual BindableProfile<TState, Tthreshold> GetSettingsForTablet(string name)
        {
            var tablet = _tablets.Find(t => t.Name == name);

            if (tablet == null)
                return null!;

            return ClickTapSettings?.Profiles.Find(p => p.Name == tablet.Name) ?? CreateProfileForTablet(tablet);
        }

        public void AddTablet(SharedTabletReference tablet)
        {
            if (_tablets.Any(t => t.Name == tablet.Name))
                return;

            _tablets.Add(tablet);
            
            // Since a new tablet has been added, we need to build the bindings for it
            BuildProfileBindings(tablet);

            TabletsChanged?.Invoke(this, _tablets);
            TabletAdded?.Invoke(this, tablet);
        }

        public void RemoveTablet(SharedTabletReference tablet)
        {
            if (!_tablets.Remove(tablet))
                return;

            TabletsChanged?.Invoke(this, _tablets);
            TabletRemoved?.Invoke(this, tablet);
        }

        public void RemoveTablet(string name)
        {
            var tablet = _tablets.Find(t => t.Name == name);

            if (tablet != null)
                RemoveTablet(tablet);
        }

        private BindableProfile<TState, Tthreshold> CreateProfileForTablet(SharedTabletReference tablet)
        {
            if (ClickTapSettings == null)
                return null!;

            var profile = new Tprofile
            {
                Name = tablet.Name
            };

            profile.MatchSpecifications(tablet);

            ClickTapSettings.Profiles.Add(profile);

            return profile;
        }

        private void BuildProfileBindings(SharedTabletReference tablet)
        {
            var profile = ClickTapSettings?.Profiles.Find(p => p.Name == tablet.Name);

            if (profile == null)
                return;

            profile.ConstructBindings(tablet);
        }

        private void LoadSettings(string path)
        {
            if (!File.Exists(path))
                SaveSettingsCore();

            HasErrored = !Settings<Tprofile, TState, Tthreshold>.TryLoadFrom(path, out var temp);

            if (!HasErrored)
                ClickTapSettings = temp;
        }

        private bool SaveSettingsCore()
        {
            Log.Write(PLUGIN_NAME, "Saving settings...");

            try
            {
                File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(ClickTapSettings, _serializerSettings));
                return true;
            }
            catch (Exception e)
            {
                Log.Write(PLUGIN_NAME, $"Failed to save settings: {e.Message}", LogLevel.Error);
            }

            return false;
        }

        #endregion

        #region Event Methods

        protected virtual void OnReady(EventArgs e)
            => Ready?.Invoke(this, e);

        protected virtual void OnSettingsChanged(Settings<Tprofile, TState, Tthreshold> settings)
            => SettingsChanged?.Invoke(this, settings);

        protected virtual void OnTabletsChanged(IEnumerable<SharedTabletReference> tablets)
            => TabletsChanged?.Invoke(this, tablets);

        protected virtual void OnTabletAdded(SharedTabletReference tablet)
            => TabletAdded?.Invoke(this, tablet);

        protected virtual void OnTabletRemoved(SharedTabletReference tablet)
            => TabletRemoved?.Invoke(this, tablet);

        #endregion

        #region RPC Methods

        /// <summary>
        ///   Gets the plugins that are available for use.
        /// </summary>
        /// <remarks>
        ///   The method of obtaining plugins depends on the version of OpenTabletDriver.
        /// </remarks>
        /// <returns>The available plugins.</returns>
        public virtual Task<List<SerializablePlugin>> GetPlugins() 
            => throw new NotImplementedException();

        /// <inheritdoc />
        public virtual Task<bool> IsTabletConnected()
        {
            Log.Write(PLUGIN_NAME, "Checking if tablet is connected...");

            return Task.FromResult(_tablets.Any());
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<SharedTabletReference>> GetTablets()
        {
            if (!_tablets.Any())
                return Task.FromResult(Array.Empty<SharedTabletReference>().AsEnumerable());

            Log.Write(PLUGIN_NAME, "Getting tablets...");

            return Task.FromResult(_tablets.AsEnumerable());
        }

        /// <inheritdoc />
        public virtual Task<SerializableSettings> GetSettings()
        {
            Log.Write(PLUGIN_NAME, "Converting Settings into a serializable form...");

            if (ClickTapSettings == null)
                return Task.FromResult<SerializableSettings>(null!);

            // TODO: Build the serializable settings from bindable (probably)
            var serializedSettings = ClickTapSettings.ToSerializable(IdentifierToPluginConversion);

            return Task.FromResult(serializedSettings);
        }

        /// <inheritdoc />
        public virtual Task<bool> SaveSettings() => Task.FromResult(SaveSettingsCore());

        /// <inheritdoc />
        public virtual Task<bool> UpdateSettings(SerializableSettings settings)
        {
            Log.Write(PLUGIN_NAME, "Updating settings...");

            if (settings == null)
                return Task.FromResult(false);

            // TODO: Build the settings from serializable
            //TouchGestureSettings = Settings.FromSerializable(settings, IdentifierToPluginConversion);
            SettingsChanged?.Invoke(this, ClickTapSettings);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public virtual Task<bool> UpdateProfile(SerializableProfile profile)
        {
            Log.Write(PLUGIN_NAME, "Updating profile...");

            if (profile == null)
                return Task.FromResult(false);

            var bindableProfile = ClickTapSettings?.Profiles.Find(p => p.Name == profile.Name);
            var tablet = _tablets.Find(t => t.Name == profile.Name);

            if (bindableProfile == null || tablet == null)
                return Task.FromResult(false);

            bindableProfile.Update(profile, tablet, IdentifierToPluginConversion);

            return Task.FromResult(true);
        }

        #endregion

        #region Disposal

        public virtual void Dispose()
        {
            _tablets.Clear();
            _rpcServer?.Dispose();

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Static Events Trigger

        protected static void OnDaemonLoaded(object? sender = null)
        {
            DaemonLoaded?.Invoke(sender, EventArgs.Empty);
        }

        #endregion
    }
}
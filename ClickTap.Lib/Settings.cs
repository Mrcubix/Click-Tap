using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin;
using ClickTap.Lib.Converters;
using ClickTap.Lib.Entities.Serializable;
using System.Reflection;
using ClickTap.Lib.Entities.Bindable;
using ClickTap.Lib.Bindings;

namespace ClickTap.Lib
{
    public class Settings<TProfile, TState, Tthreshold>
        where TProfile : BindableProfile<TState, Tthreshold>, new()
        where TState : Binding
        where Tthreshold : ThresholdBinding
    {
        #region Constants
        
        private static readonly JsonSerializerSettings _serializerSettings = new()
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

        #region Constructors

        [JsonConstructor]
        public Settings()
        {
        }

        #endregion

        #region Properties

        [JsonProperty]
        public int Version { get; set; } = 1;

        [JsonProperty]
        public List<TProfile> Profiles { get; set; } = new();

        #endregion

        #region Methods

        public static bool TryLoadFrom(string path, out Settings<TProfile, TState, Tthreshold>? settings)
        {
            settings = null!;

            if (File.Exists(path))
            {
                try
                {
                    var serialized = File.ReadAllText(path);
                    settings = JsonConvert.DeserializeObject<Settings<TProfile, TState, Tthreshold>>(serialized, _serializerSettings)!;

                    return true;
                }
                catch(Exception e)
                {
                    Log.Write("Touch Gestures Settings Loader", $"Failed to load settings from {path}: {e}", LogLevel.Error);
                }
            }
            else
            {
                Log.Write("Touch Gestures Settings Loader", $"Failed to load settings from {path}: file does not exist", LogLevel.Error);
            }

            return false;
        }

        /// <summary>
        ///   Some bindings may not have a pointer provided to them using this method.
        /// </summary>
        public void ConstructBindings()
        {
            foreach (var profile in Profiles)
                profile.ConstructBindings();
        }

        public void FromSerializable(SerializableSettings settings, Dictionary<int, TypeInfo> identifierToPlugin)
        {
            foreach (var serializableProfile in settings.Profiles)
            {
                var profile = new TProfile();
                profile.FromSerializable(serializableProfile, identifierToPlugin);

                Profiles.Add(profile);
            }
        }

        public SerializableSettings ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin)
        {
            var result = new SerializableSettings();

            foreach (var profile in Profiles)
                if (profile.ToSerializable(identifierToPlugin) is SerializableProfile serializableProfile)
                    result.Profiles.Add(serializableProfile);

            return result;
        }

        #endregion

        #region Static Properties

        public static Settings<TProfile, TState, Tthreshold> Default => new();

        #endregion
    }
}
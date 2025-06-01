using System;
using ClickTap.Lib.Tablet;
using Newtonsoft.Json;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.Lib.Entities.Serializable
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SerializableProfile : ITabletProfile
    {
        #region Properties

        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        [JsonProperty]
        public SerializablePluginSettingsStore? Tip { get; set; }

        [JsonProperty]
        public float TipActivationThreshold { get; set; }

        [JsonProperty]
        public SerializablePluginSettingsStore? Eraser { get; set; }

        [JsonProperty]
        public float EraserActivationThreshold { get; set; }

        [JsonProperty]
        public SerializablePluginSettingsStore?[] PenButtons { get; set; } = Array.Empty<SerializablePluginSettingsStore>();

        [JsonProperty]
        public SerializablePluginSettingsStore?[] AuxButtons { get; set; } = Array.Empty<SerializablePluginSettingsStore>();

        [JsonProperty]
        public SerializablePluginSettingsStore?[] MouseButtons { get; set; } = Array.Empty<SerializablePluginSettingsStore>();

        [JsonProperty]
        public SerializablePluginSettingsStore? MouseScrollUp { get; set; }

        [JsonProperty]
        public SerializablePluginSettingsStore? MouseScrollDown { get; set; }

        #endregion

        #region Methods

        public void Clear()
        {
            Tip = null;
            Eraser = null;
            Array.Fill(PenButtons, null);
            Array.Fill(AuxButtons, null);
            Array.Fill(MouseButtons, null);
            MouseScrollUp = null;
            MouseScrollDown = null;
        }

        public void MatchSpecifications(SharedTabletReference tabletSpecifications)
        {
            PenButtons = new SerializablePluginSettingsStore?[tabletSpecifications.Pen?.Buttons?.ButtonCount ?? 0];
            AuxButtons = new SerializablePluginSettingsStore?[tabletSpecifications.AuxButtons?.ButtonCount ?? 0];
            MouseButtons = new SerializablePluginSettingsStore?[tabletSpecifications.MouseButtons?.ButtonCount ?? 0];
        }

        #endregion
    }
}
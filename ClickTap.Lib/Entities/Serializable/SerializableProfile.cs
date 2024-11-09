using System;
using ClickTap.Lib.Entities.Serializable.Bindings;
using ClickTap.Lib.Tablet;
using Newtonsoft.Json;

namespace ClickTap.Lib.Entities.Serializable
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SerializableProfile : ITabletProfile
    {
        #region Properties

        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        [JsonProperty]
        public SerializableThresholdBinding? Tip { get; set; }

        [JsonProperty]
        public SerializableThresholdBinding? Eraser { get; set; }

        [JsonProperty]
        public SerializableBinding?[] PenButtons { get; set; } = Array.Empty<SerializableBinding>();

        [JsonProperty]
        public SerializableBinding?[] AuxButtons { get; set; } = Array.Empty<SerializableBinding>();

        [JsonProperty]
        public SerializableBinding?[] MouseButtons { get; set; } = Array.Empty<SerializableBinding>();

        [JsonProperty]
        public SerializableBinding? MouseScrollUp { get; set; }

        [JsonProperty]
        public SerializableBinding? MouseScrollDown { get; set; }

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
            PenButtons = new SerializableBinding?[tabletSpecifications.Pen?.Buttons?.ButtonCount ?? 0];
            AuxButtons = new SerializableBinding?[tabletSpecifications.AuxButtons?.ButtonCount ?? 0];
            MouseButtons = new SerializableBinding?[tabletSpecifications.MouseButtons?.ButtonCount ?? 0];
        }

        #endregion
    }
}
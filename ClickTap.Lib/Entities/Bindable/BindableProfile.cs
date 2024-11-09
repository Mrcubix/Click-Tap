using System;
using System.Collections.Generic;
using System.Reflection;
using ClickTap.Lib.Bindings;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Tablet;
using Newtonsoft.Json;

namespace ClickTap.Lib.Entities.Bindable
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BindableProfile<TState, Tthreshold> : ITabletProfile
        where TState : Binding 
        where Tthreshold : ThresholdBinding
    {
        public event EventHandler? ProfileChanged;

        #region Properties

        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        [JsonProperty]
        public Tthreshold? Tip { get; set; }

        [JsonProperty]
        public Tthreshold? Eraser { get; set; }

        [JsonProperty]
        public TState?[] PenButtons { get; set; } = Array.Empty<TState>();

        [JsonProperty]
        public TState?[] AuxButtons { get; set; } = Array.Empty<TState>();

        [JsonProperty]
        public TState?[] MouseButtons { get; set; } = Array.Empty<TState>();

        [JsonProperty]
        public TState? MouseScrollUp { get; set; }

        [JsonProperty]
        public TState? MouseScrollDown { get; set; }

        #endregion

        #region Methods

        public void Clear()
        {
            Tip = null!;
            Eraser = null!;
            Array.Fill(PenButtons, null);
            Array.Fill(AuxButtons, null);
            Array.Fill(MouseButtons, null);
            MouseScrollUp = null!;
            MouseScrollDown = null!;
        }

        public void MatchSpecifications(SharedTabletReference tabletSpecifications)
        {
            PenButtons = new TState?[tabletSpecifications.Pen?.Buttons?.ButtonCount ?? 0];
            AuxButtons = new TState?[tabletSpecifications.AuxButtons?.ButtonCount ?? 0];
            MouseButtons = new TState?[tabletSpecifications.MouseButtons?.ButtonCount ?? 0];
        }

        /// <summary>
        ///   Constructs the bindings for this profile using a set builder.
        /// </summary>
        /// <param name="tablet">The Tablet owning these bindings.</param>
        /// <remarks>
        ///   TODO: Apply abstraction to bindings so that we use inherited classes or builders Instead of <see cref="BindingBuilder"/>.
        /// </remarks>
        public abstract void ConstructBindings(SharedTabletReference? tablet = null);

        public abstract void Update(SerializableProfile profile, SharedTabletReference tablet, Dictionary<int, TypeInfo> identifierToPlugin);

        public abstract void FromSerializable(SerializableProfile profile, Dictionary<int, TypeInfo> identifierToPlugin, 
                                              SharedTabletReference? tablet = null);

        public abstract SerializableProfile ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin);

        #endregion

        #region Event Handlers

        public void OnProfileChanged()
        {
            ProfileChanged?.Invoke(this, null!);
        }

        #endregion

        #region Static Methods

        public static TProfile FromSerializable<TProfile>(SerializableProfile profile, Dictionary<int, TypeInfo> identifierToPlugin, 
                                                          SharedTabletReference? tablet = null, in TProfile? existingProfile = null)
            where TProfile : BindableProfile<TState, Tthreshold>, new()
        {
            var result = existingProfile ?? new TProfile();
            result.FromSerializable(profile, identifierToPlugin, tablet);

            return result;
        }

        public static SerializableProfile ToSerializable<TProfile>(TProfile profile, Dictionary<int, TypeInfo> identifierToPlugin)
            where TProfile : BindableProfile<TState, Tthreshold>
        {
            var result = new SerializableProfile();
            {
                result.Name = profile.Name;
            }

            // TODO:

            return result;
        }
        
        #endregion
    }
}
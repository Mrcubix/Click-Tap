using System;
using System.Collections.Generic;
using System.Reflection;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Tablet;
using Newtonsoft.Json;

namespace ClickTap.Lib.Entities.Bindable
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BindableProfile : ITabletProfile
    {
        public event EventHandler? ProfileChanged;

        #region Properties

        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        #endregion

        #region Methods

        /// <summary>
        ///   Constructs the bindings for this profile using a set builder.
        /// </summary>
        /// <param name="tablet">The Tablet owning these bindings.</param>
        /// <remarks>
        ///   TODO: Apply abstraction to bindings so that we use inherited classes or builders Instead of <see cref="BindingBuilder"/>.
        /// </remarks>
        public virtual void ConstructBindings(SharedTabletReference? tablet = null)
        {
            
        }

        public void Add()
        {
            
        }

        public void Remove()
        {
            
        }

        public void Clear()
        {
            
        }

        public void Update(SerializableProfile profile, SharedTabletReference tablet, Dictionary<int, TypeInfo> identifierToPlugin)
        {
            FromSerializable(profile, identifierToPlugin, tablet, this);
            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Event Handlers

        public void OnProfileChanged()
        {
            ProfileChanged?.Invoke(this, null!);
        }

        #endregion

        #region Static Methods

        public static BindableProfile FromSerializable(SerializableProfile profile, Dictionary<int, TypeInfo> identifierToPlugin, SharedTabletReference? tablet = null, in BindableProfile? existingProfile = null)  
        {
            var result = existingProfile ?? new BindableProfile();
            result.Name = profile.Name;

            if (existingProfile != null)
                result.Clear();

            // TODO: 

            return result;
        }

        public static SerializableProfile ToSerializable(BindableProfile profile, Dictionary<int, TypeInfo> identifierToPlugin)
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
using System;
using System.Collections.Generic;
using System.Reflection;
using ClickTap.Lib.Entities.Serializable;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Tablet;

namespace ClickTap.Lib.Bindings
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Binding
    {
        public Binding() {}

        public Binding(SerializableBinding binding, Dictionary<int, TypeInfo> identifierToPlugin)
            => FromSerializable(binding, identifierToPlugin);

        public bool State { protected set; get; }

        protected bool PreviousState { set; get; }

        public abstract void Press(IDeviceReport report);
        public abstract void Release(IDeviceReport report);

        public virtual void Invoke(IDeviceReport report, bool newState)
        {
            if (newState && !PreviousState)
                Press(report);
            else if (!newState && PreviousState)
                Release(report);

            PreviousState = newState;
        }

        public abstract void Construct();

        public abstract SerializableBinding ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin);

        public abstract void FromSerializable(SerializableBinding binding, Dictionary<int, TypeInfo> identifierToPlugin);
    }
}
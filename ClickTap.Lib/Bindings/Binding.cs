using System;
using System.Collections.Generic;
using System.Reflection;
using ClickTap.Lib.Entities.Serializable;
using Newtonsoft.Json;

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

        public abstract void Press();
        public abstract void Release();

        public virtual void Invoke(bool newState)
        {
            if (newState && !PreviousState)
                Press();
            else if (!newState && PreviousState)
                Release();

            PreviousState = newState;
        }

        public abstract void Construct();

        public abstract SerializableBinding ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin);

        public abstract void FromSerializable(SerializableBinding binding, Dictionary<int, TypeInfo> identifierToPlugin);
    }
}
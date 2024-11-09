using System.Collections.Generic;
using System.Reflection;
using ClickTap.Lib.Entities.Serializable;

namespace ClickTap.Lib.Binding
{
    public abstract class Binding
    {
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
    }
}
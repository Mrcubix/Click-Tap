using System.Collections.Generic;
using System.Reflection;
using ClickTap.Lib.Bindings;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Entities.Serializable.Bindings;

namespace ClickTap.Bindings
{
    public abstract class BindingBuilder
    {
        /// <summary>
        ///   Builds a ThresholdBinding from a SerializableThresholdBinding
        /// </summary>
        /// <param name="binding">The Serializable to build a proper ThresholdBinding from.</param>
        /// <param name="identifierToPlugin">The identifier to plugin mapping.</param>
        /// <returns></returns>
        public abstract ThresholdBinding? BuildThresholdFromSerializable(SerializableThresholdBinding binding, Dictionary<int, TypeInfo> identifierToPlugin);

        /// <summary>
        ///   Builds a Binding from a SerializableBinding
        /// </summary>
        /// <param name="binding">The Serializable to build a proper Binding from.</param>
        /// <param name="identifierToPlugin">The identifier to plugin mapping.</param>
        /// <returns></returns>
        public abstract Binding? BuildStateFromSerializable(SerializableThresholdBinding binding, Dictionary<int, TypeInfo> identifierToPlugin);

        /// <summary>
        ///   Builds a SerializableThresholdBinding from a ThresholdBinding
        /// </summary>
        /// <param name="binding">The ThresholdBinding to build a proper SerializableThresholdBinding from.</param>
        /// <param name="identifierToPlugin">The identifier to plugin mapping.</param>
        /// <returns></returns>
        public abstract SerializableThresholdBinding? BuildThresholdToSerializable(ThresholdBinding binding, Dictionary<int, TypeInfo> identifierToPlugin);

        /// <summary>
        ///   Builds a SerializableBinding from a Binding
        /// </summary>
        /// <param name="binding">The Binding to build a proper SerializableBinding from.</param>
        /// <param name="identifierToPlugin">The identifier to plugin mapping.</param>
        /// <returns></returns>
        public abstract SerializableBinding? BuildStateToSerializable(Binding binding, Dictionary<int, TypeInfo> identifierToPlugin);
    }
}
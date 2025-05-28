using System.Reflection;
using System.Collections;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.External.Common.Serializables.Properties;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.Extensions
{
    public static class SerializablePropertyExtension
    {
        internal static readonly Dictionary<Type, JTokenType> TypeToJTokenType = new()
        {
            { typeof(bool), JTokenType.Boolean },
            { typeof(int), JTokenType.Integer },
            { typeof(float), JTokenType.Float },
            { typeof(string), JTokenType.String },
            { typeof(IEnumerable), JTokenType.Array }
        };

        public static SerializableProperty SerializableFrom(PropertyInfo property)
        {
            ArgumentNullException.ThrowIfNull(property);

            if (TypeToJTokenType.TryGetValue(property.PropertyType, out var type) == false)
                throw new ArgumentException($"Property type {property.PropertyType} is not supported.");

            // ALL that extra reflection bs just to get valid keys
            var attribute = property.GetCustomAttribute<PropertyValidatedAttribute>();

            if (attribute == null)
                return new SerializableProperty(property.Name, type, Array.Empty<SerializableAttributeModifier>());
            else
            {
                var validKeys = attribute?.GetValue<IList>(property) ?? Array.Empty<object>();
                return new SerializableValidatedProperty(property.Name, JTokenType.Array, validKeys, Array.Empty<SerializableAttributeModifier>());
            }
        }
    }
}
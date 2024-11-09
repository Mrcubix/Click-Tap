using ClickTap.Lib.Tablet;
using Newtonsoft.Json;

namespace ClickTap.Lib.Entities
{
    public interface ITabletProfile
    {
        /// <summary>
        ///   The name of the tablet associated with the profile.
        /// </summary>
        [JsonProperty]
        string Name { get; }

        public void Clear();

        public void MatchSpecifications(SharedTabletReference tabletSpecifications);
    }
}
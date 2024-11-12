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

        /// <summary>
        ///   Set all bindings to null.
        /// </summary>
        public void Clear();

        /// <summary>
        ///   Matches the tablet specification to the tablet profile.
        /// </summary>
        /// <param name="tabletSpecifications">The tablet specification, Crafted by the handler</param>
        public void MatchSpecifications(SharedTabletReference tabletSpecifications);
    }
}
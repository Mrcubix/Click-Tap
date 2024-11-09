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
        ///   
        /// </summary>
        void Add();

        /// <summary>
        ///   
        /// </summary>
        void Remove();
    }
}
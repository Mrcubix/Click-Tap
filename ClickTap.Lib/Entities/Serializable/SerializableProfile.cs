using Newtonsoft.Json;

namespace ClickTap.Lib.Entities.Serializable
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SerializableProfile
    {
        [JsonProperty]
        public string Name { get; set; } = string.Empty;

        #region Methods

        public void Add()
        {
            
        }

        public void Remove()
        {
            
        }

        #endregion

        #region IEnumerable Implementation

        /// <returns>Returns an enumerator where all gestures are aggregated.</returns>
        /*public IEnumerator<Gesture> GetEnumerator()
        {
            foreach (var tapGesture in TapGestures)
                yield return tapGesture;

            foreach (var holdGesture in HoldGestures)
                yield return holdGesture;

            foreach (var swipeGesture in SwipeGestures)
                yield return swipeGesture;

            foreach (var panGesture in PanGestures)
                yield return panGesture;

            foreach (var pinchGesture in PinchGestures)
                yield return pinchGesture;

            foreach (var rotateGesture in RotateGestures)
                yield return rotateGesture;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }*/

        #endregion
    }
}
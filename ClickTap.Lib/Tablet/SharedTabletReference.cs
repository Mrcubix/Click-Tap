using System.Numerics;
using Newtonsoft.Json;

namespace ClickTap.Lib.Tablet
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SharedTabletReference
    {
        public SharedTabletReference()
        {
        }

        public SharedTabletReference(string name, SharedTabletDigitizer digitizer, SharedTabletDigitizer touchDigitizer, 
                                     SharedPenSpecifications? penSpecifications, SharedButtonSpecifications? auxButtons,
                                     SharedButtonSpecifications? mouseButtons)
        {
            Name = name;
            PenDigitizer = digitizer;
            TouchDigitizer = touchDigitizer;
            Pen = penSpecifications;
            AuxButtons = auxButtons;
            MouseButtons = mouseButtons;
        }

        public SharedTabletReference(string name, SharedTabletDigitizer digitizer, SharedTabletDigitizer touchDigitizer, 
                                     SharedPenSpecifications? penSpecifications, SharedButtonSpecifications? auxButtons,
                                     SharedButtonSpecifications? mouseButtons, SharedDeviceIdentifier deviceIdentifier)
        {
            Name = name;
            PenDigitizer = digitizer;
            TouchDigitizer = touchDigitizer;
            DeviceIdentifier = deviceIdentifier;
            Pen = penSpecifications;
            AuxButtons = auxButtons;
            MouseButtons = mouseButtons;
        }

        /// <summary>
        ///   The name of the tablet.
        /// </summary>
        [JsonProperty]
        public string Name { set; get; } = string.Empty;

        /// <summary>
        ///   The device identifier of the tablet.
        /// </summary>
        [JsonProperty]
        public SharedDeviceIdentifier? DeviceIdentifier { set; get; } = null;

        /// <summary>
        ///   The Pen digitizer specifications of the tablet.
        /// </summary>
        [JsonProperty]
        public SharedTabletDigitizer? PenDigitizer { set; get; } = null;

        /// <summary>
        ///   The Touch digitizer specifications of the tablet.
        /// </summary>
        [JsonProperty]
        public SharedTabletDigitizer? TouchDigitizer { set; get; } = null;

        /// <summary>
        ///   Get the Pen Specifications.
        /// </summary>
        [JsonProperty]
        public SharedPenSpecifications? Pen { get; set; } = null;

        /// <summary>
        ///   Get the the auxiliary buttons specifications.
        /// </summary>
        [JsonProperty]
        public SharedButtonSpecifications? AuxButtons { get; set; } = null;

        /// <summary>
        ///   Get the the mouse buttons specifications.
        /// </summary>
        [JsonProperty]
        public SharedButtonSpecifications? MouseButtons { get; set; } = null;

        public Vector2 Size => new(PenDigitizer!.Width, PenDigitizer!.Height);
    }
}
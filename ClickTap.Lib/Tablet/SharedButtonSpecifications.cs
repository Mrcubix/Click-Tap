using System.ComponentModel.DataAnnotations;

namespace ClickTap.Lib.Tablet
{
    public class SharedButtonSpecifications
    {
        /// <summary>
        /// The amount of buttons.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(ButtonCount)} must be defined")]
        public uint ButtonCount { set; get; }
    }
}
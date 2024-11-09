using System.ComponentModel.DataAnnotations;

namespace ClickTap.Lib.Tablet
{
    public class SharedPenSpecifications
    {
        /// <summary>
        /// The maximum pressure that the pen supports.
        /// </summary>
        [Required(ErrorMessage = $"Pen {nameof(MaxPressure)} must be defined")]
        public uint MaxPressure { set; get; }

        /// <summary>
        /// Specifications for the pen buttons.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(Buttons)} must be defined")]
        public SharedButtonSpecifications Buttons { set; get; } = new SharedButtonSpecifications();
    }
}
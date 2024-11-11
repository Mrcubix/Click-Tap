using System.Numerics;
using ClickTap.Entities;
using ClickTap.Lib.Tablet;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Tablet;

namespace ClickTap.Extensions
{
    public static class TabletReferenceExtensions
    {
        public static BulletproofSharedTabletReference ToShared(this TabletReference tablet, SharedTabletDigitizer touchDigitizer)
        {
            var digitizer = tablet?.Properties?.Specifications?.Digitizer;

            if (tablet == null || digitizer == null)
                throw new ArgumentNullException(nameof(tablet));

            var penDigitizer = new SharedTabletDigitizer
            {
                Width = digitizer.Width,
                Height = digitizer.Height,
                MaxX = digitizer.MaxX,
                MaxY = digitizer.MaxY
            };

            var penSpecifications = tablet.Properties.Specifications.Pen != null ? new SharedPenSpecifications
            {
                MaxPressure = tablet.Properties.Specifications.Pen.MaxPressure,
                Buttons = new SharedButtonSpecifications
                {
                    ButtonCount = tablet.Properties.Specifications.Pen.Buttons?.ButtonCount ?? 0,
                }
            } : null;

            var auxSpecifications = tablet.Properties.Specifications.AuxiliaryButtons != null ? new SharedButtonSpecifications
            {
                ButtonCount = tablet.Properties.Specifications.AuxiliaryButtons?.ButtonCount ?? 0
            } : null;

            var mouseSpecifications = tablet.Properties.Specifications.MouseButtons != null ? new SharedButtonSpecifications
            {
                ButtonCount = tablet.Properties.Specifications.MouseButtons?.ButtonCount ?? 0
            } : null;

            return new BulletproofSharedTabletReference(tablet.Properties.Name, penDigitizer, touchDigitizer, 
                                                        penSpecifications, auxSpecifications, mouseSpecifications, 
                                                        new ServiceManager());
        }

        public static Vector2 GetLPMM(this TabletReference tabletReference)
        {
            var digitizer = tabletReference.Properties.Specifications.Digitizer;

            return new Vector2(digitizer.MaxX / digitizer.Width, digitizer.MaxY / digitizer.Height);
        }

        public static Vector2 GetTouchLPMM(this TabletReference tabletReference, SharedTabletDigitizer touchDigitizer)
        {
            var digitizer = tabletReference.Properties.Specifications.Digitizer;

            return new Vector2(touchDigitizer.MaxX / digitizer.Width, touchDigitizer.MaxY / digitizer.Height);
        }
    }
}